using AI.Ghosts.States;
using Items.Ghosts;
using System.Collections;
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
    public float lastTimeHadLOS = -Mathf.Infinity;

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

    // player
    public Transform player;

    // child ghost UI interactable
    public Canvas childUI;

    // hallucination effects (if needed)
    public HallucinationDirector director;

    // hearing
    public GhostHearing hearing;

    [Header("Spawn Placement Fields")]
    public float forwardDistance = 6f;
    public float sideOffset = 2f;
    public float wallBackoff = 0.75f;
    public float sameFloorHeightTolerance = 1.5f;

    [Header("Respawn")]
    [SerializeField] private Transform respawnPoint;


    private Renderer[] renderers;
    private Collider[] colliders;

    private void Awake()
    {
        player = FindAnyObjectByType<PlayerController>().transform;

        context = new GhostContext(player.GetComponent<InsanitySystem>());
        stateMachine = new GhostStateMachine(this);

        renderers = GetComponentsInChildren<Renderer>(true);
        colliders = GetComponentsInChildren<Collider>(true);

        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;
        agent.updateRotation = false; // keep manual
        agent.updateUpAxis = true;
    }
    private void Start()
    {
        // initialize this ghost with the personality values initially set
        personality.InitializeGhost(this);
        agent.autoBraking = false;    // for chase; prevents weird slowdowns at destination
        agent.stoppingDistance = minDistance; // or your “attack range”
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

        if (context.playerIsHidden)
        {
            context.playerHideSpot = player.GetComponent<PlayerController>().hideSpot;
        }

        UpdateVision();
    }

    public bool IsVisible()
    {
        if (renderers == null) return false;
        foreach (var r in renderers) if (r.enabled) return true;
        return false;
    }

    public bool IsPlayerInAllowedArea()
    {
        int areaId = player.GetComponent<PlayerController>().currentHouseAreaId;
        // if the area Id of the player is not -1, then the player is in a valid house area for chasing
        return areaId != -1;
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

    public bool StillRemembersNoise()
    {
        return Time.time - context.lastHeardTime <= hearing.noiseMemorySeconds;
    }

    // movement methods
    public void MoveTo(Vector3 target)
    {
        agent.SetDestination(target);
    }

    public void RotateChase(Transform target, float facePlayerWeight = 0.7f)
    {
        // Direction agent wants to move (path direction)
        Vector3 moveDir = agent.desiredVelocity;
        moveDir.y = 0f;
        if (moveDir.sqrMagnitude > 0.001f) moveDir.Normalize();

        // Direction to player (for “phasing” / facing player)
        Vector3 toPlayer = (target.position - transform.position);
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.001f) toPlayer.Normalize();

        // Blend: mostly face player, but include movement direction so it turns tightly
        Vector3 blended = Vector3.Slerp(moveDir, toPlayer, Mathf.Clamp01(facePlayerWeight));
        if (blended.sqrMagnitude < 0.001f) return;

        Quaternion look = Quaternion.LookRotation(blended, Vector3.up);

        // IMPORTANT: use a higher turn speed than rotSpeed for chase
        float chaseTurnSpeed = rotSpeed * 4f; // tune 3–8
        transform.rotation = Quaternion.Slerp(transform.rotation, look, Time.deltaTime * chaseTurnSpeed);
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

    public void ResetGhost(bool resetToSpawn = true)
    {
        // Enter safe do-nothing state first
        stateMachine.ChangeState(GhostStateID.Idle);

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }

        // Clear runtime memory
        context = new GhostContext(player.GetComponent<PlayerController>().insanitySystem);
        lastTimeHadLOS = -Mathf.Infinity;

        // Reset emotions
        personality.InitializeGhost(this);

        if (resetToSpawn && respawnPoint != null)
        {
            WarpTo(respawnPoint.position, respawnPoint.rotation);
        }

        StartCoroutine(FinishResetAfterDelay(0.15f));
    }

    private IEnumerator FinishResetAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (agent != null && agent.enabled)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }

        GhostStateID ghostState = personality.DecideNextState(this);

        stateMachine.ChangeState(ghostState);
    }

    public void WarpTo(Vector3 pos, Quaternion rot)
    {
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;
        }

        transform.SetPositionAndRotation(pos, rot);

        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(pos);
            agent.nextPosition = pos;
            agent.isStopped = false;
            agent.ResetPath();
        }
    }


    public void ApplyGhostItem(GhostItemData data)
    {
        personality.ApplyGhostItemEffect(this, data);
    }

    public Vector3 GetVisibleSpawnPoint(Transform cam)
    {
        float playerFloorY = cam.position.y;
        Vector3 playerRayStart = cam.position + Vector3.up * 2f;

        if (Physics.Raycast(playerRayStart, Vector3.down, out var playerGroundHit, 10f, groundMask, QueryTriggerInteraction.Ignore))
            playerFloorY = playerGroundHit.point.y;

        // Try both sides so we can reject blocked ones
        float[] sides = Random.value < 0.5f ? new float[] { -1f, 1f } : new float[] { 1f, -1f };

        foreach (float side in sides)
        {
            Vector3 candidate =
                cam.position +
                cam.forward * forwardDistance +
                cam.right * (sideOffset * side);

            if (TryGetValidVisiblePoint(cam, candidate, playerFloorY, out Vector3 result))
                return result;
        }

        // Fallback: try directly in front but closer
        Vector3 fallback =
            cam.position +
            cam.forward * (forwardDistance * 0.5f);

        if (TryGetValidVisiblePoint(cam, fallback, playerFloorY, out Vector3 fallbackResult))
            return fallbackResult;

        // Last resort: return something on the player's floor in front of camera
        Vector3 emergency = cam.position + cam.forward * 2f;
        emergency.y = playerFloorY;
        return emergency;
    }

    private bool TryGetValidVisiblePoint(Transform cam, Vector3 desired, float playerFloorY, out Vector3 result)
    {
        result = desired;

        // Snap to navmesh near desired point
        if (!NavMesh.SamplePosition(desired, out NavMeshHit navHit, 2f, NavMesh.AllAreas))
            return false;

        // Must be on roughly same floor
        if (Mathf.Abs(navHit.position.y - playerFloorY) > sameFloorHeightTolerance)
            return false;

        Vector3 candidate = navHit.position;

        // Raise the ray a bit so we don't shoot straight into the floor
        Vector3 camOrigin = cam.position;
        Vector3 target = candidate + Vector3.up * 1.0f;

        Vector3 dir = target - camOrigin;
        float dist = dir.magnitude;

        if (dist <= 0.01f)
            return false;

        dir /= dist;

        // Reject if a wall/environment blocks line of sight
        if (Physics.SphereCast(camOrigin, 0.2f, dir, out RaycastHit hit, dist, environmentMask, QueryTriggerInteraction.Ignore))
        {
            return false;
        }

        result = candidate;
        return true;
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