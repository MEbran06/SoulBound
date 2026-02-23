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
        float agressiveness = controller.context.GetEmotion(EmotionType.Aggression);

        if (agressiveness >= GetThreshold(EmotionType.Aggression))
        {
            return GhostStateID.Chase;
        }

        // if player insanity falls bellow the necessary threshold for hallucinations to start
        if (sanity <= GetThreshold(EmotionType.Fear))
        {
            return GhostStateID.Hallucination;
        }

        return GhostStateID.Stalk; // stalk the player
    }

    public override void ApplyGhostItemEffect(GhostController controller, GhostItemData data)
    {
        
    }

    public override void HandleTriggerEnter(Collider other, GhostController controller)
    {
        // player loses
        FindAnyObjectByType<GameManager>().PlayerCaught(controller);
    }
}
