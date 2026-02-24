using UnityEngine;

public class HideSpot : Interactable
{
    [SerializeField] private Transform hidePosition;
    [SerializeField] private Transform exitPosition;

    private PlayerController player;
    private GhostController[] ghosts;

    private void Start()
    {
        promptMessage = "Press E to Hide";
        // find all active ghosts
        ghosts = FindObjectsByType<GhostController>(FindObjectsSortMode.None);
    }

    public override void Interact(PlayerController gPlayer)
    {
        player = gPlayer;
        if (!player.isHidden)
        {
            EnterHide();
        }
        else
        {
            ExitHide();
        }
    }

    void EnterHide()
    {
        player.characterController.enabled = false;
        player.transform.position = hidePosition.position;
        player.characterController.enabled = true;
        // store the hide spot of the player in the context of the ghosts
        foreach (GhostController ghost in ghosts)
        {
            ghost.context.playerHideSpot = this;
        }
        player.isHidden = true;
    }

    public void ExitHide()
    {
        player.characterController.enabled = false;
        player.transform.position = exitPosition.position;
        player.characterController.enabled = true;

        player.isHidden = false;
    }
}
