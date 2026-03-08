using System.Collections.Generic;
using UnityEngine;

public class HideSpot : Interactable
{
    public static readonly List<HideSpot> All = new List<HideSpot>();

    public Transform hidePosition;
    public Transform exitPosition;

    private PlayerController player;
    private GhostController[] ghosts;

    [SerializeField][Min(0f)] private float suspicion = 0f;
    [SerializeField][Min(0f)] private float baseSuspicion = 0f;
    [SerializeField][Min(0f)] private float suspicionDecayRate = 0.05f;

    private bool isPlayerHidden = false;
    public int currentAreaId;


    public float Suspicion => suspicion;

    private void OnEnable()
    {
        All.Add(this);
    }

    private void OnDisable()
    {
        All.Remove(this);
    }

    private void Start()
    {
        promptMessage = "Press E to Hide";
        // find all active ghosts
        ghosts = FindObjectsByType<GhostController>(FindObjectsSortMode.None);
    }

    void Awake()
    {
        Area houseArea = GetComponentInParent<Area>();
        if (houseArea != null)
        {
            currentAreaId = houseArea.area.houseAreaId;
        }
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
        player.hideSpot = this;
        isPlayerHidden = true;
        // add suspicion to it simply because the player entered
        IncreaseSuspicion(0.5f);
    }

    public void ExitHide()
    {
        player.characterController.enabled = false;
        Vector3 position = new Vector3(exitPosition.position.x, player.characterController.height / 2f, exitPosition.position.z);
        player.transform.position = position;
        player.characterController.enabled = true;

        player.isHidden = false;
        player.hideSpot = null;
        isPlayerHidden = false;
        DecreaseSuspicion(0.5f);
    }

    private void Update()
    {
        if (suspicion > baseSuspicion)
            DecreaseSuspicion(suspicion - suspicionDecayRate * Time.deltaTime);
    }

    public bool IsPlayerInside()
    {
        return isPlayerHidden;
    }

    public void ForceExit()
    {
        Debug.Log("Exit hide");
        player.characterController.enabled = false;
        Vector3 position = new Vector3(exitPosition.position.x, player.characterController.height/2f, exitPosition.position.z);
        player.transform.position = position;
        player.characterController.enabled = true;

        player.isHidden = false;
        isPlayerHidden = false;
    }

    public void IncreaseSuspicion(float amount)
    {
        suspicion += amount;
    }

    public void DecreaseSuspicion(float amount)
    {
        suspicion = Mathf.Max(baseSuspicion, suspicion - amount);
    }
}
