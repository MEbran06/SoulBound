using AI.Ghosts.States;
using Items.Ghosts;
using Unity.VisualScripting;
using UnityEngine;
using Ghosts.Emotions;

[CreateAssetMenu(fileName = "FatherPersonality", menuName = "Scriptable Objects/FatherPersonality")]
public class FatherPersonality : GhostPersonality
{
    public float pursuitGraceTime = 2f;

    public override GhostStateID DecideNextState(GhostController controller)
    {
        EmotionTrace lastTrace = controller.context.emotion.GetLastTrace(EmotionType.Aggression);
        Debug.Log($"Aggression: {controller.context.emotion.GetEmotion(EmotionType.Aggression)}");
        Debug.Log($"change: {lastTrace.delta} time: {lastTrace.time} source: {lastTrace.source}");

        if (controller.context.emotion.GetEmotion(EmotionType.Confusion) >= GetThreshold(EmotionType.Confusion))
        {
            return GhostStateID.Stunned;
        }

        if (controller.GetCurrentState() == GhostStateID.Chase)
        {
            bool isSafe = controller.player.GetComponent<PlayerController>().IsInSafeRoom;
            if (isSafe || !controller.IsPointInAllowedArea(controller.player.position))
                return GhostStateID.Search;
        }

        if (controller.context.canSeePlayer)
        {
            controller.lastTimeHadLOS = Time.time;
            return GhostStateID.Chase;
        }
        else if (Time.time - controller.lastTimeHadLOS < pursuitGraceTime)
        {
            return GhostStateID.Chase;
        }

        if (controller.StillRemembersPlayer())
        {
            // If we are searching and search finished, give up
            if (controller.GetCurrentState() == GhostStateID.Search &&
                controller.context.searchComplete)
                return GhostStateID.Patrol;

            controller.context.noiseTriggeredSearch = false;
            return GhostStateID.Search;
        }

        if (controller.StillRemembersNoise())
        {
            controller.context.noiseTriggeredSearch = true;
            return GhostStateID.Search;
        }

        return GhostStateID.Patrol;
    }

    public override void ApplyGhostItemEffect(GhostController controller, GhostItemData data)
    {
        foreach (var mod in data.modifiers)
        {
            float sensitivity = GetSensitivity(mod.emotion);
            controller.context.emotion.AddFromItem(mod.emotion, mod.value, sensitivity);
        }
    }

    public override void HandleTriggerEnter(Collider other, GhostController controller)
    {
        if (other.CompareTag("Player") && !controller.context.playerIsHidden)
        {
            GameManager.Instance.PlayerCaught(controller);
        }
    }


}