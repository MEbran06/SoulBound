using UnityEngine;

public class HiddenState : GhostState
{
    public HiddenState(GhostController controller) : base(controller) { }

    public override void Enter()
    {
        // make the ghost invisible
        controller.SetVisible(false);
        // End interaction window if somehow still active
        controller.context.childInteractionAllowed = false;
    }

    public override void Execute()
    {
        // nothing needed here
    }

    public override void Exit()
    {
        // nothing needed here
    }
}
