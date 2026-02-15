using UnityEngine;

public class HideSpot : Interactable
{
    [SerializeField] private Transform hidePosition;
    [SerializeField] private Transform exitPosition;

    private PlayerController player;

    private void Start()
    {
        promptMessage = "Press E to Hide";
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
        player.cc.enabled = false;
        player.transform.position = hidePosition.position;
        player.cc.enabled = true;

        player.isHidden = true;
    }

    void ExitHide()
    {
        player.cc.enabled = false;
        player.transform.position = exitPosition.position;
        player.cc.enabled = true;

        player.isHidden = false;
    }
}
