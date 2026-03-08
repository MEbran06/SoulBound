using UnityEngine;
using System.Collections.Generic;
using System;

public class CheckpointManager : MonoBehaviour
{
    [Header("References")]
    public Transform player; // drag Player transform here
    public static CheckpointManager Instance;

    [SerializeField] Transform InitialPlayerSpawn; // initially set to spawn point
    [Tooltip("The player object that has the character component attached to it")]
    [SerializeField] private Transform playerBody;
    [SerializeField] private Inventory playerInventory;

    private int lastMilestoneId = -1;
    private Vector3 bodyLocalPos0;
    private Quaternion bodyLocalRot0;

    public int activeMilestoneIdForRespawn = -1;
    public readonly string ActiveKey = "active_checkpoint_milestone";

    private void Awake()
    {
        Instance = this;
        bodyLocalPos0 = playerBody.localPosition;
        bodyLocalRot0 = playerBody.localRotation;
    }

    void Start()
    {
        // ensure lamp is given on start
        ItemSO lamp = ItemDatabase.Instance.GetByName("Lamp");
        if (lamp != null)
        {
            Inventory.Instance.AddItem(lamp, 1);
        }
        else
        {
            Debug.Log("NO LAMP");
        }

        activeMilestoneIdForRespawn = PlayerPrefs.GetInt(ActiveKey, -1);

        // Save initial checkpoint at game start if none exists
        if (!SaveSystem.TryLoadCheckpoint(out _))
        {
            SaveInitialCheckpoint();
        }
        // apply selected checkpoint on scene load
        if (SaveSystem.TryLoadCheckpointForMilestone(activeMilestoneIdForRespawn, out var data))
        {
            ApplyCheckpoint(data);
        }
        else if (SaveSystem.TryLoadCheckpointForMilestone(-1, out data))
        {
            activeMilestoneIdForRespawn = -1;
            SaveActiveMilestone();
            ApplyCheckpoint(data);
        }

        // Subscribe to milestone events
        MilestoneManager.Instance.MilestoneEntered += HandleMilestoneEntered;
    }

    void OnDestroy()
    {
        if (MilestoneManager.Instance != null)
            MilestoneManager.Instance.MilestoneEntered -= HandleMilestoneEntered;
    }

    private void SaveInitialCheckpoint()
    {
        var data = new CheckpointData
        {
            milestoneId = -1, // "start"
            position = InitialPlayerSpawn.position,
            rotation = InitialPlayerSpawn.rotation,
            currentItems = new Dictionary<Guid, SavedPickedupItemsState>(playerInventory.pickedItems), // take a snapshot of the player's inventory
            collectedWorldItemIds = WorldItemManager.Instance.GetCollectedWorldItemIds()
        };

        SaveSystem.SaveCheckpoint(data);
        lastMilestoneId = -1;
        Debug.Log("Saved Initial Checkpoint");
        // save this checkpoint
        SaveActiveMilestone();
    }

    private void HandleMilestoneEntered(int milestoneId, Transform milestoneTransform)
    {
        // Avoid saving repeatedly if the player stands in the trigger
        if (milestoneId == lastMilestoneId) return;

        var data = new CheckpointData
        {
            milestoneId = milestoneId,
            position = milestoneTransform.position,
            rotation = milestoneTransform.rotation,
            currentItems = new Dictionary<Guid, SavedPickedupItemsState>(playerInventory.pickedItems), // take a snapshot of the player's inventory
            collectedWorldItemIds = WorldItemManager.Instance.GetCollectedWorldItemIds()
        };

        SaveSystem.SaveCheckpoint(data);
        lastMilestoneId = milestoneId;
        Debug.Log($"[Checkpoint] Saved at milestone {milestoneId} pos={milestoneTransform.position}");
        activeMilestoneIdForRespawn = milestoneId;
        // save this checkpoint
        SaveActiveMilestone();
    }

    public void RespawnFromLastCheckpoint()
    {
        // Prefer the player-selected / current run checkpoint
        if (!SaveSystem.TryLoadCheckpointForMilestone(activeMilestoneIdForRespawn, out var data))
        {
            // fallback to start if something went wrong
            if (!SaveSystem.TryLoadCheckpointForMilestone(-1, out data))
            {
                Debug.LogWarning("[Checkpoint] No checkpoint found to respawn.");
                return;
            }

            activeMilestoneIdForRespawn = -1;
        }

        ApplyCheckpoint(data);
    }

    private void ApplyInventorySnapshot(Dictionary<Guid, SavedPickedupItemsState> snapshot)
    {
        // Clear current inventory dictionary
        playerInventory.pickedItems.Clear();

        if (snapshot == null)
        {
            Debug.Log("Snapshot is null");
            return;
        }
        else if (snapshot.Count == 0)
        {
            Debug.Log("Snapshot is empty");
        }


        // Copy snapshot into live dictionary
        foreach (var kv in snapshot)
        {
            Debug.Log($"key: {kv.Key} Value: {kv.Value.itemName}");
            playerInventory.pickedItems[kv.Key] = kv.Value;
        }

        // Now update the actual Slot UI based on pickedItems
        // You need to clear all slots and repopulate them using SlotID -> Slot mapping.
        foreach (var slot in FindObjectsByType<Slot>(FindObjectsSortMode.None))
            slot.ClearSlot();

        foreach (var kv in playerInventory.pickedItems)
        {
            var saved = kv.Value;

            // Look up the slot by ID 
            if (!SlotRegistry.ById.TryGetValue(Guid.Parse(saved.SlotID), out var slot) || slot == null)
            {
                Debug.Log("No slot found");
                continue;
            }
            Debug.Log("FOUND A SLOT");

            var so = ItemDatabase.Instance.GetByName(saved.itemName);
            if (so != null)
                slot.SetItem(so, saved.amount);
            else
                Debug.LogWarning($"Missing ItemSO for '{saved.itemName}'");
        }

        // refresh the inventory
        playerInventory.SendMessage("RefreshAfterInventoryChange", SendMessageOptions.DontRequireReceiver);
    }

    private void CleanupRuntimeDrops()
    {
        foreach (var d in FindObjectsByType<RuntimeDrop>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (d != null) Destroy(d.gameObject);
        }
    }

    public void LoadCheckpointByMilestone(int milestoneId)
    {
        if (!SaveSystem.TryLoadCheckpointForMilestone(milestoneId, out var data))
        {
            Debug.LogWarning($"[Checkpoint] Could not load milestone {milestoneId}");
            return;
        }
        // Move player + reset offsets + restore inventory/world items/ghosts
        activeMilestoneIdForRespawn = milestoneId;
        ApplyCheckpoint(data);
        // save the milestone choosen by player
        SaveActiveMilestone();
    }

    private void ApplyCheckpoint(CheckpointData data)
    {

        var cc = player.GetComponentInChildren<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.SetPositionAndRotation(data.position, data.rotation);

        playerBody.localPosition = bodyLocalPos0;
        playerBody.localRotation = bodyLocalRot0;

        if (cc != null) cc.enabled = true;

        // inventory restore 
        ApplyInventorySnapshot(data.currentItems);

        // cleanup runtime drops
        CleanupRuntimeDrops();

        // restore original world items to checkpoint state 
        WorldItemManager.Instance.RestoreFromCheckpoint(data.collectedWorldItemIds);
    }

    private void SaveActiveMilestone()
    {
        PlayerPrefs.SetInt(ActiveKey, activeMilestoneIdForRespawn);
        PlayerPrefs.Save();
    }

}