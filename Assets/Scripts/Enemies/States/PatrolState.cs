using UnityEngine;

public class PatrolState : GhostState
{
    private int patrolIndex = 0;
    public PatrolState(GhostController controller) : base(controller) {}

    public override void Execute()
    {
        Vector3 moveDir = controller.agent.desiredVelocity;

        if (moveDir.sqrMagnitude < 0.01f)
            moveDir = controller.transform.forward;

        Quaternion baseRot = Quaternion.LookRotation(moveDir);
        float offset = Mathf.Sin(Time.time * controller.speed) * controller.searchAngle;
        baseRot *= Quaternion.Euler(0, offset, 0);

        // move towards the current target
        controller.MoveTo(GetCurrentPatrolPoint());
        controller.RotateTo(baseRot);

        if (!controller.agent.pathPending &&
        controller.agent.remainingDistance < controller.context.maxTargetDistance)
        {
            AdvancePatrolPoint();
        }

    }
    public override void Enter() 
    {
        Debug.Log("Patrolling");
    }
    public override void Exit() {}

    public Vector3 GetCurrentPatrolPoint()
    {
        // simply return the ghost's current position 
        if (controller.patrolPoints.Length == 0)
            return controller.transform.position;

        return controller.patrolPoints[patrolIndex].position;
    }

    public void AdvancePatrolPoint()
    {
        patrolIndex = (patrolIndex + 1) % controller.patrolPoints.Length;
    }
}