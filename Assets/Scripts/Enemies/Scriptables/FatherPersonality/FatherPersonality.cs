using AI.Ghosts.States;
using Items.Ghosts;
using UnityEngine;

[CreateAssetMenu(fileName = "FatherPersonality", menuName = "Scriptable Objects/FatherPersonality")]
public class FatherPersonality : GhostPersonality
{
    [Range(0f, 100f)] public float aggressionThreshold;
    [Range(0f, 100f)] public float confusionThreshold;
    public override GhostStateID DecideNextState(GhostController controller)
    {
        GhostContext context = controller.context;

        if (controller.context.GetEmotion(EmotionType.Confusion) >= confusionThreshold)
        {
            Debug.Log("Stunned");
            return GhostStateID.Stunned;
        }

        // follow the player if you see it
        if (context.canSeePlayer)
        {
            return GhostStateID.Chase;
        }

        // follow the player if you still remember it
        if (controller.StillRemembersPlayer())
        {
            return GhostStateID.Chase;
        }

        // keep patrolling otherwise
        return GhostStateID.Patrol;
    }

    public override void ApplyGhostItemEffect(GhostController controller, GhostItemData data)
    {
        Debug.Log("Applying the effect on the ghost");
        foreach (var mod in data.modifiers)
        {
            float sensitivity = GetSensitivity(mod.emotion);
            controller.context.ModifyEmotion(mod.emotion, mod.value * sensitivity);
        }
    }

    private float GetSensitivity(EmotionType emotion)
    {
        // sensitivities will be part of the scriptable object, and initialized on the inspector
        foreach (var s in sensitivities)
        {
            if (s.emotion == emotion)
                return s.multiplier;
        }

        return 1f; // default multiplier if not found
    }


}
