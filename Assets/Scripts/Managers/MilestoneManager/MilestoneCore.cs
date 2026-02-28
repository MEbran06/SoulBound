using UnityEngine;

public class MilestoneCore : MonoBehaviour
{
    public MilestoneDefinition milestoneDefinition;

    // Prevent multiple enters from multiple player colliders / jitter
    private bool playerInside;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (playerInside) return;

        playerInside = true;

        // Optional: guard against missing manager during scene loads
        if (MilestoneManager.Instance == null) return;

        MilestoneManager.Instance.OnEnterMilestone(milestoneDefinition.milestoneID);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;

        if (MilestoneManager.Instance == null) return;
        MilestoneManager.Instance.OnExitMilestone(milestoneDefinition.milestoneID);
    }
}