using AI.Ghosts.States;
using Items.Ghosts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "ChildPersonality", menuName = "Scriptable Objects/ChildPersonality")]
public class ChildPersonality : GhostPersonality
{
    [Header("Milestone Variables")]
    public int lastProcessedEnterToken = -1;
    public int lastProcessedSummonToken = -1;
    [Tooltip("How long the child stays in request state")]
    public float summonRequestWindow = 5f;
    [Tooltip("Cooldown to avoid request spams")]
    public float requestCooldown = 3f;

    public float appearDuration = 2f;
    bool childHasActiveRequest;
    int childRequestedItemId;
    float nextRequestTime;
    public float maxRequestDistance = 30f;

    public override GhostStateID DecideNextState(GhostController controller)
    {
        GhostStateID current = controller.GetCurrentState();
        Debug.Log($"Attachment (child): {controller.context.emotion.GetEmotion(Ghosts.Emotions.EmotionType.Attachment)}");

        // Hidden state
        if (current == GhostStateID.Hidden)
        {
            // Hidden -> Appear 
            if (MilestoneManager.Instance != null &&
                MilestoneManager.Instance.enterToken != lastProcessedEnterToken)
            { 
                lastProcessedEnterToken = MilestoneManager.Instance.enterToken;

                int milestoneID = MilestoneManager.Instance.currentMilestoneId;

                if (milestoneID >= 0 &&
                    milestoneID == MilestoneManager.Instance.activeMilestoneId &&
                    MilestoneManager.Instance.runtimeById.TryGetValue(milestoneID, out var ms) &&
                    ms.ChildAppearPlayed == false)
                {
                    ms.ChildAppearPlayed = true;
                    MilestoneManager.Instance.runtimeById[milestoneID] = ms;
                    return GhostStateID.Appear;
                }
            }

            // Hidden -> Request
            if (GameManager.Instance != null &&
                GameManager.Instance.ChildSummonToken != lastProcessedSummonToken &&
                Time.time < GameManager.Instance.childSummonRequestTime + summonRequestWindow &&
                SafeToManifest(controller) &&
                controller.context.CanEnterRequestWindow())
            {
                lastProcessedSummonToken = GameManager.Instance.ChildSummonToken;
                return GhostStateID.Request;
            }

            return GhostStateID.Hidden;
        }

        // Appear state
        if (current == GhostStateID.Appear)
        {
            if (GameManager.Instance == null) return GhostStateID.Hidden;

            if (!SafeToManifest(controller)) return GhostStateID.Hidden;

            if (GameManager.Instance.ChildAppearedTime + appearDuration < Time.time)
                return GhostStateID.Hidden;

            return GhostStateID.Appear;
        }

        // Request state
        if (current == GhostStateID.Request)
        {
            // End Request based on child window, not milestone.
            if (!SafeToManifest(controller)) return GhostStateID.Hidden;

            //if (!controller.context.childInteractionAllowed)
            //    return GhostStateID.Hidden;

            if (Time.time > GameManager.Instance.childSummonRequestTime + summonRequestWindow)
                return GhostStateID.Hidden;

            return GhostStateID.Request;
        }

        return GhostStateID.Hidden;
    }

    private bool SafeToManifest(GhostController controller)
    {
        if (GameManager.Instance.isPlayerBeingChased)
            return false;

        if (controller.context.playerIsHidden)
            return false; // optional: child doesn't appear if player hiding

        return true;
    }

    public bool TryPickNearestRequestedItem(GhostController controller, out GhostItemData chosenData)
    {
        chosenData = null;

        if (controller == null || controller.player == null) return false;

        Vector3 playerPos = controller.player.position;

        // Find all GhostItem instances currently in the scene
        GameObject[] childGhostItems = GameManager.Instance.childGhostItems;

        float bestDistSq = float.PositiveInfinity;
        GhostItemData best = null;

        foreach (var item in childGhostItems)
        {
            if (item == null) continue;

            var data = item.GetComponent<GhostItemData>();
            if (data == null) continue;

            // Eligible for specific requests
            if (data.childItemId < 0) continue;

            float dSq = (item.transform.position - playerPos).sqrMagnitude;
            // continue if you go above the max
            if (dSq > maxRequestDistance * maxRequestDistance) continue;
            if (dSq < bestDistSq)
            {
                bestDistSq = dSq;
                best = data;
            }
        }

        if (best == null) return false;

        chosenData = best;
        return true;
    }

    public override void ApplyGhostItemEffect(GhostController controller, GhostItemData data)
    {
    }

    public override void HandleTriggerEnter(Collider other, GhostController controller)
    {
        if (!other.CompareTag("Player")) return;
        if (!controller.context.childInteractionAllowed) return;

        controller.childUI?.gameObject.SetActive(true);
    }

    public override void HandleTriggerExit(Collider other, GhostController controller)
    {
        if (!other.CompareTag("Player")) return;
        controller.childUI?.gameObject.SetActive(false);
    }
}
