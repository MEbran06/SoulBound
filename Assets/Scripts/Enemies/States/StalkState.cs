using Items.Ghosts;
using UnityEngine;
using Ghosts.Emotions;

public class StalkState : GhostState
{
    Transform[] manifestationPoints;
    Transform currentPoint;

    float nextRepositionTime;

    // New: move only when player looks away
    bool wasLooking = false;
    float nextMoveAllowedTime = 0f;
    bool lookAwayQueued = false;

    // Tuning knobs
    const float DOT_FOV = 0.65f;                // wide cone for looking
    const float MOVE_COOLDOWN_MIN = 0.8f;
    const float MOVE_COOLDOWN_MAX = 1.8f;

    public StalkState(GhostController controller) : base(controller) { }

    public override void Enter()
    {
        // make it stop if it was moving
        controller.StopMoving();
        controller.SetVisible(false);

        if (manifestationPoints == null)
            manifestationPoints = getPoints(controller);

        // First reveal
        nextRepositionTime = Time.time + Random.Range(0.8f, 1.8f);

        wasLooking = false;
        nextMoveAllowedTime = Time.time; // allow movement once visible
        currentPoint = null;
    }

    public override void Execute()
    {
        // Calm aggression while in stalk mode
        float calmRate = controller.personality.aggressionDecayRate;
        float mult = controller.context.difficulty.Get(DifficultyChannel.AggressionRate);
        controller.context.emotion.AddFromAI(EmotionType.Aggression, -calmRate * Time.deltaTime, mult);

        if (manifestationPoints == null || manifestationPoints.Length == 0)
            return;

        // If not visible yet, do initial reveal on timer
        if (!controller.IsVisible())
        {
            if (Time.time < nextRepositionTime) return;

            Transform first = FindClosestVisiblePoint(controller, manifestationPoints, exclude: currentPoint);
            if (first != null)
            {
                controller.transform.position = first.position;
                currentPoint = first;
                // make ghost look at player
                Vector3 look = controller.player.position - controller.transform.position;
                look.y = 0f;
                if (look.sqrMagnitude > 0.01f)
                    controller.transform.rotation = Quaternion.LookRotation(look);

                controller.SetVisible(true);

                // Give a small grace period so ghost doesn't instantly move away on first frame
                nextMoveAllowedTime = Time.time + Random.Range(0.3f, 0.6f);
            }
            else
            {
                nextRepositionTime = Time.time + Random.Range(0.5f, 1.2f);
            }

            return;
        }

        bool isLooking = IsPlayerLooking(controller);
        //Debug.Log($"wasLooking={wasLooking}, isLooking={isLooking}, nextMoveAllowedTime={nextMoveAllowedTime:0.00}, now={Time.time:0.00}");

        // Detect the transition (looking -> not looking) and latch it
        if (wasLooking && !isLooking)
        {
            lookAwayQueued = true;
        }

        // If player looks at ghost again, clear the queued move
        if (isLooking)
        {
            lookAwayQueued = false;
        }

        // If player just stopped looking, relocate (instead of vanishing)
        if (lookAwayQueued && Time.time >= nextMoveAllowedTime)
        {
            Transform next = FindClosestVisiblePoint(controller, manifestationPoints, exclude: currentPoint);

            if (next != null)
            {
                controller.transform.position = next.position;
                currentPoint = next;

                // make ghost look at player
                Vector3 look = controller.player.position - controller.transform.position;
                look.y = 0f;
                if (look.sqrMagnitude > 0.01f)
                    controller.transform.rotation = Quaternion.LookRotation(look);
            }
            else
            {
                controller.SetVisible(false);
            }

            lookAwayQueued = false; // consume the event
            nextMoveAllowedTime = Time.time + Random.Range(MOVE_COOLDOWN_MIN, MOVE_COOLDOWN_MAX);
        }

        wasLooking = isLooking;
    }

    public override void Exit() 
    {
        controller.SetVisible(false );
    }

    // Point visibility: camera -> point not occluded by environment
    private bool IsPointVisible(Vector3 from, Vector3 to, LayerMask occluders)
    {
        Vector3 dir = to - from;
        float dist = dir.magnitude;
        if (dist < 0.01f) return true;
        dir /= dist;

        return !Physics.Raycast(from, dir, dist, occluders, QueryTriggerInteraction.Ignore);
    }

    private Transform FindClosestVisiblePoint(GhostController controller, Transform[] points, Transform exclude)
    {
        Camera cam = Camera.main;
        if (cam == null) return null;

        float bestDist = Mathf.Infinity;
        Transform best = null;

        for (int i = 0; i < points.Length; i++)
        {
            var p = points[i];
            if (p == null) continue;
            if (exclude != null && p == exclude) continue;

            if (!IsPosWithinFOV(cam.transform, p.position, DOT_FOV))
                continue;

            if (!IsPointVisible(cam.transform.position, p.position, controller.environmentMask))
                continue;

            float d = Vector3.Distance(p.position, controller.player.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = p;
            }
        }

        return best;
    }

    private bool IsPosWithinFOV(Transform cam, Vector3 pos, float dotMin)
    {
        Vector3 to = pos - cam.position;
        if (to.sqrMagnitude < 0.001f) return true;
        Vector3 dir = to.normalized;
        return Vector3.Dot(cam.forward, dir) >= dotMin;
    }

    private Transform[] getPoints(GhostController controller)
    {
        GameObject[] pointObjects = GameObject.FindGameObjectsWithTag("ManifestationPoints");
        if (pointObjects == null || pointObjects.Length == 0) return null;

        Transform[] points = new Transform[pointObjects.Length];
        for (int i = 0; i < pointObjects.Length; i++)
            points[i] = pointObjects[i].transform;

        return points;
    }

    private bool IsPlayerLooking(GhostController controller)
    {
        Camera cam = Camera.main;
        if (cam == null) return false;

        Transform camTransform = cam.transform;

        Vector3 toGhost = (controller.transform.position - camTransform.position);
        float distance = toGhost.magnitude;
        if (distance < 0.01f) return true;

        Vector3 dirToGhost = toGhost / distance;

        float dot = Vector3.Dot(camTransform.forward, dirToGhost);
        if (dot < DOT_FOV)
            return false;

        // LOS check (fix: allow child hits + use environment mask)
        if (Physics.Raycast(camTransform.position, dirToGhost, out RaycastHit hit, distance, ~0, QueryTriggerInteraction.Ignore))
        {
            // If we hit something that's NOT part of the ghost, view is blocked
            if (!hit.transform.IsChildOf(controller.transform))
                return false;
        }

        return true;
    }
}