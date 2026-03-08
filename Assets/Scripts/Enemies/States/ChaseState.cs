// using UnityEditor.Profiling;
using UnityEngine;

public class ChaseState : GhostState
{
    const float MAX_DISTANCE_FROM_HIDING = 3f;
    float ghostSpeed = 1.0f;
    public ChaseState(GhostController controller) : base(controller) { }

    public override void Execute()
    {
        if (controller.context.playerIsHidden &&
        controller.context.playerHideSpot != null &&
        controller.StillRemembersPlayer())
        {
            HideSpot spot = controller.context.playerHideSpot;

            controller.agent.SetDestination(spot.transform.position);

            if (!controller.agent.pathPending &&
                controller.agent.remainingDistance <= 1.2f)
            {
                if (spot.IsPlayerInside())
                {
                    spot.ForceExit();
                    controller.context.lastTimePlayerSeen = Time.time;
                    controller.context.lastKnownPlayerPosition = controller.player.position;
                }
            }

            return;
        }


        // 1) Predict target a bit (reduces jukes)
        Vector3 targetPos = controller.player.position;
        var cc = controller.player.GetComponent<CharacterController>();
        Vector3 v = (cc != null) ? cc.velocity : Vector3.zero;

        float leadTime = Mathf.Clamp(controller.context.distanceToPlayer / 8f, 0.05f, 0.25f);
        targetPos += v * leadTime;

        // 2) Move agent toward predicted target (updates every frame)
        controller.MoveTo(targetPos);

        // 3) Face a blend of where we’re moving and where the player is
        Vector3 moveDir = controller.agent.desiredVelocity;
        moveDir.y = 0f;
        if (moveDir.sqrMagnitude < 0.001f)
            moveDir = (targetPos - controller.transform.position);

        moveDir.y = 0f;

        Vector3 toPlayer = (controller.player.position - controller.transform.position);
        toPlayer.y = 0f;

        if (moveDir.sqrMagnitude > 0.001f && toPlayer.sqrMagnitude > 0.001f)
        {
            moveDir.Normalize();
            toPlayer.Normalize();

            // IMPORTANT: use a constant blend, not dt-scaled
            float facePlayerWeight = 0.75f; // tune 0.6–0.9
            Vector3 blended = Vector3.Slerp(moveDir, toPlayer, facePlayerWeight);

            Quaternion rot = Quaternion.LookRotation(blended, Vector3.up);

            // IMPORTANT: rotate faster during chase
            float chaseTurnSpeed = controller.rotSpeed * 4f; // tune 3–8
            controller.transform.rotation = Quaternion.Slerp(
                controller.transform.rotation, rot, Time.deltaTime * chaseTurnSpeed);
        }

        // 4) Speed logic
        float agression01 = 1 + controller.context.emotion.Get01(Ghosts.Emotions.EmotionType.Aggression);
        float mult = controller.context.difficulty.Get(DifficultyChannel.ChaseCooldown);
        Debug.Log($"Father Chase Speed: {ghostSpeed * agression01 * mult}");
        controller.SetSpeed(ghostSpeed * agression01 * mult);
    }

    public override void Exit()
    {
        GameManager.Instance.isPlayerBeingChased = false;

        controller.agent.autoBraking = false;    // back to your default behavior
        controller.agent.acceleration = 30f;     // match what you set in Start()

        // reset the  speed
        controller.SetSpeed(ghostSpeed);
    }
    public override void Enter()
    {
        GameManager.Instance.isPlayerBeingChased = true;

        controller.agent.isStopped = false;      // IMPORTANT (in case HardStop was used)
        controller.agent.autoBraking = true;     // prevents overshoot close to target
        controller.agent.acceleration = 40f;     // tighter response (try 30–60)

        // we're chasing because we can see them
        controller.context.lastTimePlayerSeen = Time.time;
        ghostSpeed = controller.GetSpeed();
    }
}