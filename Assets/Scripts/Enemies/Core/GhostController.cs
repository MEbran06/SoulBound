using AI.Ghosts.States;
using Items.Ghosts;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor.Rendering.LookDev;
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
    public float searchSpeed = 2f;
    public float rotSpeed = 5f;
    public Transform[] patrolPoints;
    // minimum distance between ghost and target position in order to move
    public float minDistance = 0.3f;

    // player
    public Transform player;

    private void Awake()
    {
        context = new GhostContext();
        stateMachine = new GhostStateMachine(this);

        // get components
        agent = GetComponent<NavMeshAgent>();
        player = FindAnyObjectByType<PlayerController>().transform;
    }

    private void Start()
    {
        stateMachine.ChangeState(GhostStateID.Patrol);
    }

    private void Update()
    {
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
            ApplyMemory(data);
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

        UpdateVision();
    }

    void UpdateVision()
    {
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

    public void ApplyMemory(GhostItemData data)
    {
        personality.ApplyGhostItemEffect(this, data);
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
