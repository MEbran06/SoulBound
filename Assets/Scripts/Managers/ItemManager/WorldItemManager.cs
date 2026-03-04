using System.Collections.Generic;
using UnityEngine;

public class WorldItemManager : MonoBehaviour
{
    public static WorldItemManager Instance { get; private set; }

    [System.Serializable]
    public struct SpawnInfo
    {
        public string worldId;
        public string itemName;
        public int amount;
        public Vector3 position;
        public Quaternion rotation;
        public GameObject prefab; // item.itemPrefab
    }

    private readonly Dictionary<string, SpawnInfo> spawnsById = new();
    private readonly Dictionary<string, Item> liveById = new();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        CaptureAllSceneItems();
    }

    public void CaptureAllSceneItems()
    {
        spawnsById.Clear();
        liveById.Clear();

        // include inactive too, in case you have some disabled in the scene
        var items = FindObjectsByType<Item>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var it in items)
        {
            if (it == null) continue;
            if (it.item == null) continue;
            if (it.item.itemPrefab == null) continue;

            var id = it.WorldId;
            if (string.IsNullOrEmpty(id)) continue;

            // record live reference
            liveById[id] = it;

            // record spawn only once
            if (!spawnsById.ContainsKey(id))
            {
                spawnsById[id] = new SpawnInfo
                {
                    worldId = id,
                    itemName = it.item.itemName,
                    amount = it.amount,
                    position = it.transform.position,
                    rotation = it.transform.rotation,
                    prefab = it.item.itemPrefab
                };
            }
        }

        Debug.Log($"[WorldItemManager] Captured {spawnsById.Count} item spawns.");
    }

    public List<string> GetCollectedWorldItemIds()
    {
        var collected = new List<string>();

        // If it's not currently alive in the scene, we treat it as collected
        foreach (var kv in spawnsById)
        {
            string id = kv.Key;

            // If we have a live ref and it's active, it's not collected
            if (liveById.TryGetValue(id, out var live) && live != null)
                continue;

            // otherwise, collected
            collected.Add(id);
        }

        return collected;
    }

    public void NotifyCollected(Item item)
    {
        if (item == null) return;
        var id = item.WorldId;
        if (string.IsNullOrEmpty(id)) return;

        // it’s about to be destroyed, so remove it from live map
        if (liveById.TryGetValue(id, out var cur) && cur == item)
            liveById.Remove(id);
    }

    public void RestoreFromCheckpoint(List<string> collectedWorldItemIds)
    {
        var collectedSet = new HashSet<string>(collectedWorldItemIds ?? new List<string>());

        // destroy anything that should be collected but is alive
        foreach (var id in collectedSet)
        {
            if (liveById.TryGetValue(id, out var live) && live != null)
            {
                Destroy(live.gameObject);
                liveById.Remove(id);
            }
        }

        // ensure anything NOT collected exists
        foreach (var kv in spawnsById)
        {
            var id = kv.Key;
            if (collectedSet.Contains(id)) continue;

            // already alive?
            if (liveById.TryGetValue(id, out var live) && live != null)
                continue;

            var spawn = kv.Value;
            if (spawn.prefab == null) continue;

            var go = Instantiate(spawn.prefab, spawn.position, spawn.rotation);
            go.layer = LayerMask.NameToLayer("Interactables");

            // Ensure Item fields
            var it = go.GetComponent<Item>();
            if (it != null)
            {
                it.item = it.item ?? null; // usually prefab already has it
                it.amount = spawn.amount;
            }

            // preserve the SAME world id so future saves match
            it.SetWorldId = id;

            liveById[id] = it;
        }
    }

}