using UnityEditor.Profiling;
using UnityEngine; 

public class ChaseState : GhostState
{
    const float MAX_DISTANCE_FROM_HIDING = 3f;
    float ghostSpeed = 1.0f;
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
        // since aggression is 0-1 shift it up by 1 to make it have a gain on the overall speed
        float agression01 = 1 + controller.context.emotion.Get01(Ghosts.Emotions.EmotionType.Aggression);
        // control speed from attachment: high attachment -> lower value, low attachment -> higher value
        float mult = controller.context.difficulty.Get(DifficultyChannel.ChaseCooldown);
        // modify the speed based on the aggression level of the ghost and a multipler
        Debug.Log($"Father Ghost Speed: {ghostSpeed * agression01 * mult}");
        controller.SetSpeed(ghostSpeed * agression01 * mult);

        // if the player is hidden, but ghost still remembers where the player went, force player out of hidding
        if (controller.context.playerIsHidden && 
            Time.time < controller.rememberPlayerTime + controller.context.lastTimePlayerSeen)
        {
            // detect the HideSpot
            var hideSpot = controller.context.playerHideSpot; // set by HideSpot when player enters
            if (hideSpot != null)
            {
                if (Vector3.Distance(controller.transform.position, hideSpot.transform.position) < MAX_DISTANCE_FROM_HIDING)
                {
                    hideSpot.ExitHide();
                }
            }
        }
    }

    public override void Exit() 
    {
        GameManager.Instance.isPlayerBeingChased = false;
        // reset the  speed
        controller.SetSpeed(ghostSpeed);
    }
    public override void Enter() 
    {
        GameManager.Instance.isPlayerBeingChased = true;
        // we're chasing because we can see them
        controller.context.lastTimePlayerSeen = Time.time;
        ghostSpeed = controller.GetSpeed();
    }
}