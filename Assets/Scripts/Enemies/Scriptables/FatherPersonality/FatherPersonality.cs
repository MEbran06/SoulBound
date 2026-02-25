using AI.Ghosts.States;
using Items.Ghosts;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "FatherPersonality", menuName = "Scriptable Objects/FatherPersonality")]
public class FatherPersonality : GhostPersonality
{
    public float catchRadius = 1.0f; 
    public override GhostStateID DecideNextState(GhostController controller)
    {

        if (controller.context.GetEmotion(EmotionType.Confusion) >= GetThreshold(EmotionType.Confusion))
        {
            return GhostStateID.Stunned;
        }

        // follow the player if you see it
        if (controller.context.canSeePlayer)
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
        foreach (var mod in data.modifiers)
        {
            float sensitivity = GetSensitivity(mod.emotion);
            controller.context.ModifyEmotion(mod.emotion, mod.value * sensitivity);
        }
    }

    public override void HandleTriggerEnter(Collider other, GhostController controller)
    {
        if (other.CompareTag("Player") 
            && controller.context.GetEmotion(EmotionType.Aggression) > GetThreshold(EmotionType.Aggression))
        {
            GameManager.Instance.PlayerCaught(controller);
        }
    }


}
