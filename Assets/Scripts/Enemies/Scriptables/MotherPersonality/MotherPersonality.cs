using AI.Ghosts.States;
using Items.Ghosts;
using UnityEngine;

[CreateAssetMenu(fileName = "MotherPersonality", menuName = "Scriptable Objects/MotherPersonality")]
public class MotherPersonality : GhostPersonality
{
    [Header("Attack Player on Insanity")]
    [Range(0f, 100f)] public float AttackOnInsanityLevel;
    public float DistanceToAttack;
    public float AppearanceRadius;
    public float MaxAppearanceRadius;
    public override GhostStateID DecideNextState(GhostController controller)
    {
        float insanity = controller.context.insanitySystem.CurrentInsanity;
        // if player insanity falls bellow the necessary threshold for hallucinations to start
        if (insanity <= GetThreshold(EmotionType.Fear))
        {
            return GhostStateID.Hallucination;
        }

        return GhostStateID.Hallucination; // or Patrol, depending on your base state
    }

    public override void ApplyGhostItemEffect(GhostController controller, GhostItemData data)
    {
        
    }

    public override void HandleTriggerEnter(Collider other, GhostController controller)
    {
        float insanity = controller.context.insanitySystem.CurrentInsanity;
        
        if (insanity <= AttackOnInsanityLevel)
            // player loses
            FindAnyObjectByType<GameManager>().PlayerCaught(controller);
    }
}
