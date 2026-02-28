using UnityEngine;

public class PlayerSummonGhost : MonoBehaviour
{

    [SerializeField] float summonCooldown = 5f;
    private float nextSummonAllowedTime = -Mathf.Infinity;
    GameObject childGhost;
    PlayerController controller;

    void Start()
    {
        childGhost = GameObject.FindGameObjectWithTag("ChildGhost");
        controller = GetComponent<PlayerController>();
    }

    
    void Update()
    {
        if (childGhost == null) return;
        if (GameManager.Instance == null) return;
        if (MilestoneManager.Instance == null) return;

        // summon child with R
        if (Input.GetKeyDown(KeyCode.R) && IsSafeToSummon())
        {
            Debug.Log("Child Summoned!");
            GameManager.Instance.childSummonRequestTime = Time.time;
            GameManager.Instance.ChildSummonToken++;
            nextSummonAllowedTime = Time.time + summonCooldown;
        }
    }

    private bool IsSafeToSummon()
    {
        // Cooldown
        if (Time.time < nextSummonAllowedTime)
            return false;

        // Don't allow during chase
        if (GameManager.Instance.isPlayerBeingChased)
            return false;

        if (controller.isHidden)
            return false;

        return true;
    }
}
