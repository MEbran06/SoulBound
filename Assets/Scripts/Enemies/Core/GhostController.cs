using AI.Ghosts.States;
using Items.Ghosts;
using UnityEngine;
using UnityEngine.AI;

public class GhostController : MonoBehaviour
{
    // context + planning
    private GhostStateMachine stateMachine;
    public NavMeshAgent agent;
    public GhostContext context;
    // Personality (handle changing states)
    public GhostPersonality personality;

    // perception
    public float viewDistance = 10f;
    // full width of FOV
    public float viewAngle = 90f;
    [Range(1f, 2f)]
    public float lineOfSight = 1.5f;

    // memory
    public float rememberPlayerTime = 5f;

    // movement
    public float searchAngle = 25f;
    public float speed = 2f;
    public float rotSpeed = 5f;
    public Transform[] patrolPoints;
    // minimum distance between ghost and target position in order to move
    public float minDistance = 0.3f;
    public float spawnRadius = 5f;

    public LayerMask environmentMask;
    public LayerMask groundMask;
    public LayerMask interactMask;

    // player
    public Transform player;

    // child ghost UI interactable
    public Canvas childUI;

    // hallucination effects (if needed)
    public HallucinationDirector director;

    [Header("Spawn Placement Fields")]
    public float forwardDistance = 6f;
    public float sideOffset = 2f;
    public float clearanceRadius = 0.4f;
    public float clearanceHeight = 1.0f;
    public float wallBackoff = 0.75f;

    private Renderer[] renderers;
    private Collider[] colliders;

    private void Awake()
    {
        context = new GhostContext(player.GetComponent<InsanitySystem>());
        stateMachine = new GhostStateMachine(this);
        renderers = GetComponentsInChildren<Renderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);

        // get components
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        player = FindAnyObjectByType<PlayerController>().transform;
    }

    private void Start()
    {
        // initialize this ghost with the personality values initially set
        personality.InitializeGhost(this);

        stateMachine.ChangeState(GhostStateID.Patrol);
    }

    private void Update()
    {
        // stop everything if the game is over
        if (FindAnyObjectByType<GameManager>().isGameOver) return;

        UpdateContext();

        // decide what state to choose based on the personality
        GhostStateID nextState = personality.DecideNextState(this);

        // change state on the state machine if we're on a new state
        if (stateMachine.CurrentStateID != nextState)
        {
            stateMachine.ChangeState(nextState);
        }

        // execute the state
        stateMachine.Update();
    }

    private void OnEnable()
    {
        GhostItem.OnMemoryActivated += HandleMemoryActivated;
    }

    private void OnDisable()
    {
        GhostItem.OnMemoryActivated -= HandleMemoryActivated;
    }

    // check if ghost is within an activation radius
    private void HandleMemoryActivated(GhostItemData data, Vector3 position)
    {
        float distance = Vector3.Distance(transform.position, position);

        if (distance <= data.activationRadius)
        {
            ApplyGhostItem(data);
        }
    }



    void UpdateContext()
    {
        if ( player == null) return;

        // check if player is hidden
        context.playerIsHidden =
            player.GetComponent<PlayerController>().isHidden;

        // calculate the distance to the player
        context.distanceToPlayer =
            Vector3.Distance(transform.position, player.position);

        // gradually decrease confusion
        //context.ModifyEmotion(EmotionType.Confusion, context.confusionDecayRate * Time.deltaTime);


        UpdateVision();
    }

    public bool IsVisible()
    {
        if (renderers == null) return false;
        foreach (var r in renderers) if (r.enabled) return true;
        return false;
    }

    public void SetVisible(bool visible)
    {
        if (renderers == null) return;
        foreach (var r in renderers) r.enabled = visible;
        // also disable/enable the collider
        foreach (var c in colliders) c.enabled = visible;

    }

    void UpdateVision()
    {
        // assume omnipresence if the ghost cannot be seen
        if (!IsVisible())
        {
            context.canSeePlayer = true;
            return;
        }

        // Ghost can't see the player if it's hidden
        if (context.playerIsHidden)
        {
            context.canSeePlayer = false;
            return;
        }

        Vector3 dirToPlayer =
            (player.position - transform.position).normalized;

        // ghost can't see player if it's too far away
        if (context.distanceToPlayer > viewDistance)
        {
            context.canSeePlayer = false;
            return;
        }

        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        // ghost can't see player if it's not withing FOV
        if (angle > viewAngle * 0.5f)
        {
            context.canSeePlayer = false;
            return;
        }

        // get eye level of the enemy
        Vector3 eyeLine = new Vector3(transform.position.x, transform.position.y * lineOfSight, transform.position.z);

        // if we reached here then the player must be within range, so raycast to see if we hit something
        if (Physics.Raycast(eyeLine, dirToPlayer, out RaycastHit hit, viewDistance))
        {
            // we found the player
            if (hit.transform == player)
            {
                context.canSeePlayer = true;
                context.lastKnownPlayerPosition = player.position;
                context.lastTimePlayerSeen = Time.time;
            }
            else
                // we did not find the player
                context.canSeePlayer = false;
        }
    }

    // helper function to determine if the ghost should still remember where player was
    public bool StillRemembersPlayer()
    {
        return Time.time < (context.lastTimePlayerSeen + rememberPlayerTime);
    }

    // movement methods
    public void MoveTo(Vector3 target)
    {
        if (Vector3.Distance(agent.destination, target) > minDistance)
            agent.SetDestination(target);
    }

    public float GetSpeed()
    { return agent.speed; }

    public void SetSpeed(float speed) 
    { agent.speed = speed; }



    public void RotateTo(Quaternion target)
    {
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            target,
            Time.deltaTime * rotSpeed
        );
    }

    public void StopMoving()
    {
        agent.ResetPath();
    }

    public void HardStop()
    {
        if (!agent || !agent.enabled) return;

        agent.isStopped = true;
        agent.ResetPath();

        // Kill motion immediately
        agent.velocity = Vector3.zero;

        // Keep agent's internal position from "catching up" and pushing you forward
        agent.nextPosition = agent.transform.position;
    }

    public void ApplyGhostItem(GhostItemData data)
    {
        personality.ApplyGhostItemEffect(this, data);
    }

    public Vector3 GetVisibleSpawnPoint(Transform cam)
    {
        float side = Random.value < 0.5f ? -1f : 1f;

        Vector3 desired =
            cam.position +
            cam.forward * forwardDistance +
            cam.right * (sideOffset * side);

        // wall avoid camera -> desired
        Vector3 camPos = cam.position;
        Vector3 toDesired = desired - camPos;
        float dist = toDesired.magnitude;
        Vector3 dir = toDesired / Mathf.Max(dist, 0.001f);

        if (Physics.Raycast(camPos, dir, out var wallHit, dist, environmentMask, QueryTriggerInteraction.Ignore))
            desired = wallHit.point - dir * wallBackoff;

        // snap to ground at final XZ
        Vector3 rayStart = new Vector3(desired.x, cam.position.y + 10f, desired.z);
        if (Physics.Raycast(rayStart, Vector3.down, out var groundHit, 50f, groundMask, QueryTriggerInteraction.Ignore))
            desired.y = groundHit.point.y;

        //  clearance check (don't spawn inside props)
        Vector3 clearanceCenter = desired + Vector3.up * clearanceHeight;
        if (Physics.CheckSphere(clearanceCenter, clearanceRadius, environmentMask, QueryTriggerInteraction.Ignore))
        {
            // simple fallback: move closer to camera a bit
            desired -= cam.forward * 1.5f;
        }

        return desired;
    }

    public GhostStateID GetCurrentState()
    {
        return stateMachine.CurrentStateID;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward;

        Gizmos.DrawRay(transform.position, leftBoundary * viewDistance);
        Gizmos.DrawRay(transform.position, rightBoundary * viewDistance);
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
    }

}
