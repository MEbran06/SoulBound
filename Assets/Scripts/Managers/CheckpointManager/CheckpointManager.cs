using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [Header("References")]
    public Transform player; // drag Player transform here
    public static CheckpointManager Instance;

    [SerializeField] Transform InitialPlayerSpawn; // initially set to spawn point
    [Tooltip("The player object that has the character component attached to it")]
    [SerializeField] private Transform playerBody;

    private int lastMilestoneId = -1;
    private Vector3 bodyLocalPos0;
    private Quaternion bodyLocalRot0;

    private void Awake()
    {
        Instance = this;
        bodyLocalPos0 = playerBody.localPosition;
        bodyLocalRot0 = playerBody.localRotation;
    }

    void Start()
    {
        // Save initial checkpoint at game start if none exists
        if (!SaveSystem.TryLoadCheckpoint(out _))
        {
            SaveInitialCheckpoint();
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
            rotation = InitialPlayerSpawn.rotation
        };

        SaveSystem.SaveCheckpoint(data);
        lastMilestoneId = -1;
        Debug.Log("Saved Initial Checkpoint");
    }

    private void HandleMilestoneEntered(int milestoneId, Transform milestoneTransform)
    {
        // Avoid saving repeatedly if the player stands in the trigger
        if (milestoneId == lastMilestoneId) return;

        var data = new CheckpointData
        {
            milestoneId = milestoneId,
            position = milestoneTransform.position,
            rotation = milestoneTransform.rotation
        };

        SaveSystem.SaveCheckpoint(data);
        lastMilestoneId = milestoneId;
        Debug.Log($"[Checkpoint] Saved at milestone {milestoneId} pos={milestoneTransform.position}");
    }

    public void RespawnFromLastCheckpoint()
    {
        if (!SaveSystem.TryLoadCheckpoint(out var data))
        {
            Debug.Log("Falling back to current position");
            // If somehow missing, fallback to current position
            return;
        }

        // For CharacterController: disable then re-enable to avoid "stuck" issues
        var cc = player.GetComponentInChildren<CharacterController>();
        if (cc != null) 
        {
            Debug.Log("Found character");
            cc.enabled = false; 
        }

        player.SetPositionAndRotation(data.position, data.rotation);

        // reset the player's offset
        playerBody.localPosition = bodyLocalPos0;
        playerBody.localRotation = bodyLocalRot0;

        if (cc != null) cc.enabled = true;
    }
}