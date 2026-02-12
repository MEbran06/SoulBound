using UnityEngine;
using UnityEngine.AI;

public class GhostAI : MonoBehaviour
{
    public Transform player; 
    // we want to assign the ghosts a path for them to patrol in an area of the house
    [SerializeField] Transform[] pathPoints;
    [SerializeField] float viewDistance = 10f;
    [SerializeField] float maxRemainingDistance = 0.5f;
    [SerializeField] float distanceThreshold = 0.5f;
    // max number of seconds that needs to pass before the enemy forgets where the player was
    [SerializeField] float rememberPlayer = 5f;
    [Range(1f, 2f)]
    [SerializeField] float lineOfSight = 1.5f;
    [Range(0f, 360f)]
    [SerializeField] float viewAngle = 90f; // full width of the field of view

    // search variables
    [SerializeField] float searchAngle = 25f;
    [SerializeField] float searchSpeed = 2f;
    [SerializeField] float rotSpeed = 5f;


    private NavMeshAgent agent;
    private int currentPoint = 0;
    private float lastTimePlayerSeen = -999f; // initially the enemy has never seen the player
    private bool isPlayerVisible = false;

    // keep track of the enemy's state
    enum EnemyState {PATROL, FOLLOW};
    private EnemyState enemyState;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
        //agent.updateRotation = false;

    }

    void Update() {
        // first check if the player is visible
        CheckIfPlayerVisible();
        if (isPlayerVisible)
        {
            lastTimePlayerSeen = Time.time;
            enemyState = EnemyState.FOLLOW;
        }
        // if we didn't see the player but enough seconds have not pass for the enemy to forget
        else if (Time.time < lastTimePlayerSeen + rememberPlayer)
        {
            enemyState = EnemyState.FOLLOW;
        }
        else
        {
            // keep patroling
            enemyState = EnemyState.PATROL;
        }
        // repeadetly call our enemy state manager.
        ManageEnemyState();
        // handle rotation depending whether the player is in FOV or not
        Rotate();
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


    public void CheckIfPlayerVisible()
    {
        isPlayerVisible = false;
        // get the player's distance from us
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // don't even bother with Raycasting if we're not close enough
        if (distanceToPlayer <= viewDistance)   
        {
            // get the angle between the enemy's foward direction and the player
            float angle = Vector3.Angle(transform.forward, directionToPlayer);
            // get eye level of the enemy
            Vector3 eyeLine = new Vector3(transform.position.x, transform.position.y * lineOfSight, transform.position.z);

            // check that the player is in the field of view
            if (angle < viewAngle* 0.5f)
            {
                RaycastHit hit;
                // Casts a ray from enemy forward towards player
                if (Physics.Raycast(eyeLine, directionToPlayer, out hit, viewDistance))
                {
                    if (hit.transform == player)
                    {
                        isPlayerVisible = true;
                    }
                }
            }
        }
    }


    public void ManageEnemyState()
    {
        switch (enemyState)
        {
            case EnemyState.FOLLOW:
                EnemyFollow();
                break;
            case EnemyState.PATROL:
                EnemyPatrol();
                break;
            default:
                EnemyPatrol();
                break;
        }
    } 

    public void EnemyFollow()
    {
        if (Vector3.Distance(agent.destination, player.position) > distanceThreshold) {
            agent.SetDestination(player.position);
        }
    }

    public void EnemyPatrol()
    {
        if (pathPoints.Length == 0) return;

        agent.SetDestination(pathPoints[currentPoint].position);

        // move to the next point if we're within a distance
        if (!agent.pathPending && agent.remainingDistance < maxRemainingDistance)
        {
            currentPoint = (currentPoint + 1) % pathPoints.Length;
        }
    }

    private void Rotate()
    {
        Vector3 moveDirection = agent.desiredVelocity;

        Quaternion baseRotation = Quaternion.LookRotation(moveDirection);

        // scan left and right to some degree to check if the player is there
        if (enemyState == EnemyState.PATROL)
        {
            if (moveDirection.sqrMagnitude < 0.01f)
                return;
            // calculate how much to rotate left or right
            float angleOffset = Mathf.Sin(Time.time * searchSpeed) * searchAngle;
            baseRotation *= Quaternion.Euler(0, angleOffset, 0);
        }
        else if (enemyState == EnemyState.FOLLOW)
        {
            // if the player is in front of the enemy simply use our foward directions
            if (moveDirection.sqrMagnitude < 0.01f)
                moveDirection = transform.forward;
            // player direction
            Vector3 directionToPlayer = player.position - transform.position;
            directionToPlayer.y = 0f;

            // transition from the position the agent is moving to right now to face in the direction of the player
            Vector3 finalDirection = Vector3.Lerp(
                moveDirection.normalized,
                directionToPlayer.normalized,
                0.5f * Time.deltaTime // step
            );

            baseRotation = Quaternion.LookRotation(finalDirection);
        }


        // rotate to some direction at rotSpeed
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            baseRotation,
            Time.deltaTime * rotSpeed
        );
    }

}
