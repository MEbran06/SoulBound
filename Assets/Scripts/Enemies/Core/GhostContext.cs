using UnityEngine;
public class GhostContext
{
    public bool canSeePlayer;
    //public bool canHearPlayer;

    public Vector3 lastKnownPlayerPosition;
    public float lastTimePlayerSeen;
    //public float lastTimePlayerHeard;

    public float distanceToPlayer;
    public float awarenessLevel;

    public float aggression;
    public float fear;
    public float trust;
    public float suspicion;
    public Vector3 currentTargetPosition;
    public float timeEnteredState;
    public bool playerIsHidden;
    // max distance between the ghost and the target point
    public float maxTargetDistance = 0.5f;

    const float maxEmotionValue = 100f;
    
    /*
    public bool playerMadeNoise;
    public bool ghostIsSearchingClosets;
    */

    public GhostContext()
    {
        // initial assumptions
        canSeePlayer = false;
        lastKnownPlayerPosition = Vector3.zero;
        lastTimePlayerSeen = -999f; // never seen the player before
        distanceToPlayer = -1f; // undefined
        awarenessLevel = 0f;
        aggression = 0f;
        fear = 0f;
        trust = 0f;
        suspicion = 0f;
        timeEnteredState = -999f; // we don't know this yet
        playerIsHidden = false;

    }
}