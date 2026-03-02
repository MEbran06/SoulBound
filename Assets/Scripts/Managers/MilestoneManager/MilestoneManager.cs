using System;
using System.Collections.Generic;
using UnityEngine;

public enum MilestoneStatus
{
    NotStarted,
    InProgress,
    Completed
}

public struct MilestoneRuntimeState
{
    public int id;
    public bool ChildAppearPlayed;
    public MilestoneStatus status;
    public Transform spawnPosition;
}

public class MilestoneManager : MonoBehaviour
{
    public static MilestoneManager Instance;

    public int currentMilestoneId = -1;  // player location
    public int activeMilestoneId = -1;   // story lock (the only milestone allowed)
    public int enterToken = 0;

    public Dictionary<int, MilestoneRuntimeState> runtimeById;

    // we'll use events to handle saving checkpoint data or any other system that triggers on milestone
    public event Action<int, Transform> MilestoneEntered;

    // define milestone order in inspector
    [SerializeField] private List<int> milestoneOrder = new List<int>();
    public int activeIndex = 0;

    private void Awake()
    {
        Instance = this;
        runtimeById = new Dictionary<int, MilestoneRuntimeState>();
        CreateMilestoneList();

        // Pick first milestone in order
        if (milestoneOrder.Count > 0)
        {
            activeIndex = 0;
            activeMilestoneId = milestoneOrder[activeIndex];
        }
    }

    private void Start()
    {
        //Debug.Log($"Activate milestone: {activeMilestoneId}");
    }

    private void CreateMilestoneList()
    {
        MilestoneCore[] milestones = FindObjectsByType<MilestoneCore>(FindObjectsSortMode.None);

        foreach (MilestoneCore milestone in milestones)
        {
            int id = milestone.milestoneDefinition.milestoneID;

            MilestoneRuntimeState runtimeState = new MilestoneRuntimeState
            {
                id = id,
                ChildAppearPlayed = false,
                status = MilestoneStatus.NotStarted,
                spawnPosition = milestone.spawnPosition
            };

            runtimeById.TryAdd(id, runtimeState);
        }
    }

    public void OnEnterMilestone(int milestoneId, Transform spawnPos)
    {

        currentMilestoneId = milestoneId;

        MilestoneEntered?.Invoke(milestoneId, spawnPos);

        // Only the active milestone produces the appear token
        if (milestoneId != activeMilestoneId) return;

        enterToken++; // child can consume this for Appear

        if (!runtimeById.TryGetValue(milestoneId, out var s)) return;
        if (s.status == MilestoneStatus.Completed) return;

        // Mark milestone as in progress (story-wise)
        s.status = MilestoneStatus.InProgress;
        runtimeById[milestoneId] = s;

        Debug.Log("Triggered the milestone");
    }

    public void OnExitMilestone(int milestoneId, Transform spawnPos)
    {

        if (currentMilestoneId == milestoneId)
            currentMilestoneId = -1;
    }

    public void ResolveMilestone(int milestoneId)
    {

        if (milestoneId == -1) return;
        if (!runtimeById.TryGetValue(milestoneId, out var s)) return;

        s.status = MilestoneStatus.Completed;
        runtimeById[milestoneId] = s;

        if (milestoneId == activeMilestoneId)
        {
            activeIndex++;
            activeMilestoneId = (activeIndex < milestoneOrder.Count) ? milestoneOrder[activeIndex] : -1;
        }
    }
}
