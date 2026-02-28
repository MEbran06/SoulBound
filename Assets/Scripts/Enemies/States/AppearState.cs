using UnityEngine;

public class AppearState : GhostState
{
    public AppearState(GhostController controller) : base(controller) { }
    public override void Enter()
    {
        // Start the appear timer used by DecideNextState()
        if (GameManager.Instance != null)
            GameManager.Instance.ChildAppearedTime = Time.time;

        // Appear is ambient only (no interaction)
        controller.context.childInteractionAllowed = false;
        controller.childUI?.gameObject.SetActive(false);

        // spawn child, make sure it's visible
        Transform cam = controller.player.GetComponentInChildren<Camera>().transform;
        Vector3 position = controller.GetVisibleSpawnPoint(cam);
        controller.gameObject.transform.position = position;
        controller.SetVisible(true);
    }

    public override void Execute()
    {
        // nothing needed
    }

    public override void Exit()
    {
        // set back to invisible
        controller.SetVisible(false);
        // Ensure UI is off
        controller.childUI?.gameObject.SetActive(false);
    }
}
