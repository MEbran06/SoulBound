using AI.Ghosts.States;
using Items.Ghosts;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "MotherPersonality", menuName = "Scriptable Objects/MotherPersonality")]
public class MotherPersonality : GhostPersonality
{
    public override GhostStateID DecideNextState(GhostController controller)
    {
        float sanity = controller.context.insanitySystem.CurrentInsanity;
        float aggressiveness = controller.context.GetEmotion(EmotionType.Aggression);
        Debug.Log($"Aggressiveness: {controller.context.GetEmotion(EmotionType.Aggression)}");
        bool remembersPlayer = Time.time < controller.context.lastTimePlayerSeen + controller.rememberPlayerTime;
        bool shouldChase = controller.context.canSeePlayer || remembersPlayer;

        if (aggressiveness >= GetThreshold(EmotionType.Aggression))
        {
            return shouldChase ? GhostStateID.Chase : GhostStateID.Stalk;
        }
        else
        {
            if (sanity <= GetThreshold(EmotionType.Fear))
                return GhostStateID.Hallucination;

            return GhostStateID.Stalk;
        }
    }

    public override void ApplyGhostItemEffect(GhostController controller, GhostItemData data)
    {
        
    }

    public override void HandleTriggerEnter(Collider other, GhostController controller)
    {
        // player loses
        controller.HardStop();
        FindAnyObjectByType<GameManager>().PlayerCaught(controller);
    }
}
