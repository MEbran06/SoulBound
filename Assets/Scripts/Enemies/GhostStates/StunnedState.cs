using UnityEngine;
using Items.Ghosts;

public class StunnedState : GhostState
{
    // controls for how long the Ghost will be confused
    float confusionRate = -2f;
    public StunnedState(GhostController controller) : base(controller) { }

    public override void Enter()
    {
        controller.StopMoving();
    }

    public override void Execute()
    {
        controller.StopMoving();

        controller.context.ModifyEmotion(EmotionType.Confusion, confusionRate * Time.deltaTime);
    }

    public override void Exit() {}
}
