using UnityEngine;

public class RequestState : GhostState
{
    ChildPersonality child;
    public RequestState(GhostController controller) : base(controller) 
    {
        child = controller.personality as ChildPersonality;
    }

    public override void Enter()
    {
        if (child == null) return;

        ChildInteraction ci = controller.GetComponent<ChildInteraction>();
        if (ci != null)
        {
            ci.BeginRequestWindow();
        }

        // open interaction window
        controller.context.childInteractionAllowed = true;

        // Generate/refresh specific request based on nearest item
        if (!controller.context.childHasActiveRequest)
        {
            if (child.TryPickNearestRequestedItem(controller, out var data))
            {
                controller.context.childHasActiveRequest = true;
                controller.context.childRequestedItemId = data.childItemId;
            }
            else
            {
                controller.context.childHasActiveRequest = false;
                controller.context.childRequestedItemId = -1;
            }
        }

        // spawn + visible
        Transform cam = controller.player.GetComponentInChildren<Camera>().transform;
        Vector3 pos = controller.GetVisibleSpawnPoint(cam);
        controller.agent.enabled = false;
        controller.transform.position = pos;
        controller.agent.enabled = true;
        // always look at the player
        controller.transform.LookAt(controller.player.transform.position);
        controller.SetVisible(true);
    }

    public override void Execute()
    {
        // nothing to do here, child interactin script handles receiving the item
    }

    public override void Exit()
    {
        // close the interaction window
        controller.context.childInteractionAllowed = false;

        // Hide UI
        controller.childUI?.gameObject.SetActive(false);

        // Hide ghost
        controller.SetVisible(false);

        // Small cooldown to prevent spam
        controller.context.childNextAllowedRequestTime = Time.time + child.requestCooldown;

    }
}
