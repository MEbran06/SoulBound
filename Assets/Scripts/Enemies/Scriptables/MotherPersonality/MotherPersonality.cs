using AI.Ghosts.States;
using Items.Ghosts;
using UnityEngine;
using UnityEngine.AI;
using Ghosts.Emotions;

[CreateAssetMenu(fileName = "MotherPersonality", menuName = "Scriptable Objects/MotherPersonality")]
public class MotherPersonality : GhostPersonality
{
    public override GhostStateID DecideNextState(GhostController controller)
    {
        float sanity = controller.context.insanitySystem.CurrentInsanity;
        float aggressiveness = controller.context.emotion.GetEmotion(EmotionType.Aggression);
        Debug.Log($"Aggressiveness: {controller.context.emotion.GetEmotion(EmotionType.Aggression)}");
        bool remembersPlayer = Time.time < controller.context.lastTimePlayerSeen + controller.rememberPlayerTime;
        bool shouldChase = controller.context.canSeePlayer || remembersPlayer;

        if (aggressiveness >= GetThreshold(EmotionType.Aggression))
        {
            Debug.Log($"Should Chase? {shouldChase}");
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
        foreach (var mod in data.modifiers)
        {
            float sensitivity = GetSensitivity(mod.emotion);
            controller.context.emotion.AddFromItem(mod.emotion, mod.value, sensitivity);
        }

    }

    public override void HandleTriggerEnter(Collider other, GhostController controller)
    {
        // player loses
        controller.HardStop();
        GameManager.Instance.PlayerCaught(controller);
    }
}
