using UnityEditor.Profiling;
using UnityEngine; 

public class ChaseState : GhostState
{
    public ChaseState(GhostController controller) : base(controller) {}

    public override void Execute()
    {
        Vector3 moveDirection = controller.agent.desiredVelocity;
       // if the player is in front of the enemy simply use our foward directions
        if (moveDirection.sqrMagnitude < 0.01f)
            moveDirection = controller.transform.forward;
        // player direction
        Vector3 directionToPlayer = controller.player.position - controller.transform.position;
        directionToPlayer.y = 0f;

        // transition from the position the agent is moving to right now to face in the direction of the player
        Vector3 finalDirection = Vector3.Lerp(
            moveDirection.normalized,
            directionToPlayer.normalized,
            0.5f * Time.deltaTime // step
        );

        Quaternion baseRotation = Quaternion.LookRotation(finalDirection);
        controller.RotateTo(baseRotation);
        controller.MoveTo(controller.player.position);
    }

    public override void Exit() {}
    public override void Enter() {}
}