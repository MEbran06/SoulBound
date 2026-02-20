using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int Seed = 123;
    public bool isGameOver = false;
    [SerializeField] PlayerController player;
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

        // StartCoroutine(LoseSequence()); // Play animation, show Game Over UI, etc
    }
}
