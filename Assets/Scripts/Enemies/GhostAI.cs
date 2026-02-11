using UnityEngine;
using UnityEngine.AI;

public class GhostAI : MonoBehaviour
{
    public Transform player; 
    // we want to assign the ghosts a path for them to patrol in an area of the house
    [SerializeField] Transform[] pathPoints;
    private NavMeshAgent agent;
    private int currentPoint = 0;
    private float viewDistance = 10f;

    // keep track of the enemies' state
    enum EnemyState {PATROL, FOLLOW};
    [SerializeField] EnemyState enemyState;

    void Start() {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update() {
        // repeadetly call our enemy state manager.
        ManageEnemyState();
    }

    private void ManageEnemyState()
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
        if (player != null) {
            agent.SetDestination(player.position);
        }
    }

    public void EnemyPatrol()
    {
        // get the player's distance from us
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        RaycastHit hit;
        // Casts a ray from enemy forward towards player
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, viewDistance))
        {
            if (hit.transform == player)
            {
                Debug.Log("Player in sight!");
                Debug.DrawRay(transform.position, directionToPlayer * distanceToPlayer, Color.green);
            }
            else
            {
                Debug.Log("Obstacle in the way: " + hit.transform.name);
                Debug.DrawRay(transform.position, directionToPlayer * distanceToPlayer, Color.red);
            }
        }
    }
}
