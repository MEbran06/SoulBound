using AI.Ghosts.States;
using Items.Ghosts;
using UnityEngine;
using Ghosts.Emotions;

[CreateAssetMenu(fileName = "MotherPersonality", menuName = "Scriptable Objects/MotherPersonality")]
public class MotherPersonality : GhostPersonality
{
    public override GhostStateID DecideNextState(GhostController controller)
    {
        var ctx = controller.context;

        float aggressiveness = ctx.emotion.GetEmotion(EmotionType.Aggression);

        bool isPlayerSafe = controller.player.GetComponent<PlayerController>().IsInSafeRoom;

        GhostStateID currentState = controller.GetCurrentState();
        // Stay in Chase while commit window is active
        if (currentState == GhostStateID.Chase &&
            Time.time < ctx.attackCommittedUntilTime)
        {
            return GhostStateID.Chase;
        }

        // Stay in Stalk while reveal window is active
        if (currentState == GhostStateID.Stalk &&
            Time.time < ctx.nextAllowedRevealTime)
        {
            return GhostStateID.Stalk;
        }

        // Chase latch: once committed, stay in Chase briefly
        if (Time.time < ctx.attackCommittedUntilTime)
            return GhostStateID.Chase;

        // Safe room blocks Chase, but Mom can still pressure
        if (isPlayerSafe)
            return GhostStateID.Pressure;

        // Strong pressure beat requested an attack and chase is plausible
        if (ctx.pressureAttackQueued &&
            aggressiveness >= GetThreshold(EmotionType.Aggression))
        {
            ctx.pressureAttackQueued = false;
            return GhostStateID.Chase;
        }

        // Rare reveal request
        if (ctx.pressureRevealQueued)
        {
            ctx.pressureRevealQueued = false;
            return GhostStateID.Stalk;
        }

        // Default = Pressure
        return GhostStateID.Pressure;
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
        controller.HardStop();
        GameManager.Instance.PlayerCaught(controller);
    }
}