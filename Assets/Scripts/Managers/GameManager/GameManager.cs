using Items.Ghosts;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public int Seed = 123;
    public bool isGameOver = false;
    public bool isPlayerBeingChased = false;
    [SerializeField] PlayerController player;

    public float childSummonRequestTime = -Mathf.Infinity;
    public int ChildSummonToken = 0;
    public float ChildAppearedTime = -Mathf.Infinity;
    public GameObject[] childGhostItems = null;

    public GhostController childGhost;
    public float ChildAttachment01 =>
        childGhost != null
            ? childGhost.context.emotion.Get01(Ghosts.Emotions.EmotionType.Attachment)
            : 0.5f;

    private void Awake()
    {
        Instance = this;
        // get all the ghost items that the child can summon
        childGhostItems = GameObject.FindGameObjectsWithTag("ChildGhostItem");
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Random.InitState(Seed);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayerCaught(GhostController ghost)
    {
        if (isGameOver) return;

        isGameOver = true;

        player.InputDisabled = true;
        ghost?.StopMoving();

        // restart from last saved checkpoint
        RestartGame();
        // StartCoroutine(LoseSequence()); // Play animation, show Game Over UI, etc
    }

    public void RestartGame()
    {
        if (!isGameOver) return;

        CheckpointManager.Instance.RespawnFromLastCheckpoint();
        GhostManager.Instance.ResetAllGhostsAfterCaught();
        isGameOver = false;
        player.InputDisabled = false;
        player.insanitySystem.ResetSanity();
        player.currentHouseAreaId = -1;

        LanternSystem lantern = player != null ? player.GetComponent<LanternSystem>() : null;
        if (lantern != null)
        {
            lantern.SetOn(true);
        }
    }

    public void GameCompleted()
    {
        isGameOver = true;
        SaveSystem.ClearAll(); // clear all chekpoint data
        Cursor.lockState = CursorLockMode.None; // unlock cursor
        Time.timeScale = 1f; // make sure time is restored
        // load the start menu scene
        SceneManager.LoadScene("Menu");
    }
}
