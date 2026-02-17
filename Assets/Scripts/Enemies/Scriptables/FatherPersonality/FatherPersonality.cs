using AI.Ghosts.States;
using UnityEngine;

[CreateAssetMenu(fileName = "FatherPersonality", menuName = "Scriptable Objects/FatherPersonality")]
public class FatherPersonality : GhostPersonality
{
    [Range(0f, 100f)] public float aggressionThreshold;
    public override GhostStateID DecideNextState(GhostController controller)
    {
        GhostContext context = controller.context;

        // follow the player if you see it
        if (context.canSeePlayer)
        {
            return GhostStateID.Chase;
        }

        // follow the player if you still remember it
        if (controller.StillRemembersPlayer())
        {
            return GhostStateID.Chase;
        }


        // keep patrolling otherwise
        return GhostStateID.Patrol;
    }
}
