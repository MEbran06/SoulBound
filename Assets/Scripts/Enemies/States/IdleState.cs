using UnityEngine;

public class IdleState : GhostState
{
    public IdleState(GhostController controller) : base(controller) { }

    public override void Enter()
    {
        Debug.Log("ENTERED IDLE");
        if (controller.agent != null && controller.agent.enabled)
        {
            controller.agent.isStopped = true;
            controller.agent.ResetPath();
        }
    }

    public override void Execute()
    {
        // Intentionally do nothing
    }

    public override void Exit()
    {
        if (controller.agent != null && controller.agent.enabled)
        {
            controller.agent.isStopped = false;
        }
    }
}