using UnityEngine;
using System.Collections.Generic;

public class SearchState : GhostState
{
    private enum Phase { TravelToOrigin, Scan, CheckHideSpots }

    private Phase phase;

    private List<HideSpot> plan;
    private int idx;

    // Tuning knobs 
    private const float SEARCH_DURATION = 10f;
    private const float SEARCH_RADIUS = 12f;
    private const float ARRIVAL_THRESHOLD = 1.2f;

    // Scan tuning
    private const float SCAN_STEP_DEGREES = 35f;
    private const float SCAN_TURN_SPEED_MULT = 1.25f; // uses controller.rotSpeed through RotateTo
    private const float SCAN_HOLD_TIME = 0.25f;       // pause at each scan angle
    private const float SCAN_ANGLE_EPS = 3f;

    private float scanHoldUntil;
    private int scanStepIndex;
    private Quaternion scanTargetRot;

    // Optional probability check tuning
    private const float SUSPICION_MAX = 5f;
    private const float MIN_CHECK_P = 0.50f;
    private const float MAX_CHECK_P = 0.95f;

    public SearchState(GhostController controller) : base(controller) { }

    public override void Enter()
    {
        controller.agent.isStopped = false;

        controller.context.searchComplete = false;
        controller.context.searchEndTime = Time.time + SEARCH_DURATION;

        phase = Phase.TravelToOrigin;
        idx = 0;
        plan = null;

        // Scan init
        scanHoldUntil = 0f;
        scanStepIndex = 0;
        scanTargetRot = controller.transform.rotation;

        if (controller.context.noiseTriggeredSearch)
        {
            Debug.Log("Heard Something");
            controller.context.lastKnownPlayerPosition = controller.context.lastHeardPosition;
        }

        // Start moving toward last known immediately
        controller.agent.SetDestination(controller.context.lastKnownPlayerPosition);
    }

    public override void Execute()
    {
        // If we see player, search is done; decision tree will switch to Chase
        if (controller.context.canSeePlayer)
        {
            controller.context.searchComplete = true;
            return;
        }

        // Time up -> done
        if (Time.time > controller.context.searchEndTime)
        {
            controller.context.searchComplete = true;
            return;
        }

        // player hid right after LOS was lost -> check hide spot first
        if (controller.context.playerIsHidden &&
            controller.context.playerHideSpot != null &&
            Time.time - controller.context.lastTimePlayerSeen < 0.75f)
        {
            Debug.Log("Inside the hide spot");
            phase = Phase.CheckHideSpots;

            // make sure the plan starts with the player's hide spot
            EnsureHideSpotFirst(controller.context.playerHideSpot);
        }

        switch (phase)
        {
            case Phase.TravelToOrigin:
                TickTravelToOrigin();
                break;

            case Phase.Scan:
                TickScan();
                break;

            case Phase.CheckHideSpots:
                TickHideSpotChecks();
                break;
        }
    }

    public override void Exit()
    {
        plan?.Clear();
        plan = null;
    }

    // ---- Travel to last known ----
    private void TickTravelToOrigin()
    {
        controller.agent.isStopped = false;
        controller.agent.SetDestination(controller.context.lastKnownPlayerPosition);

        if (HasArrived(ARRIVAL_THRESHOLD))
        {
            phase = Phase.Scan;
            BeginScan();
        }
    }

    // ---- Scan ----
    private void BeginScan()
    {
        scanStepIndex = 0;
        scanHoldUntil = 0f;

        // We scan: left, right, center (3 steps)
        SetScanTargetForStep(scanStepIndex);
    }

    private void TickScan()
    {
        // If we have a hold timer (pause at angle), wait
        if (Time.time < scanHoldUntil)
            return;

        // Rotate toward current scan target
        controller.RotateTo(scanTargetRot);

        float angle = Quaternion.Angle(controller.transform.rotation, scanTargetRot);

        if (angle <= SCAN_ANGLE_EPS)
        {
            // Hold briefly at this angle (lets UpdateVision run + feels intentional)
            scanHoldUntil = Time.time + SCAN_HOLD_TIME;

            scanStepIndex++;
            if (scanStepIndex >= 3)
            {
                // Done scanning -> hide spot checks
                phase = Phase.CheckHideSpots;
                BuildPlan();
                return;
            }

            SetScanTargetForStep(scanStepIndex);
        }
    }

    private void SetScanTargetForStep(int step)
    {
        float yaw = 0f;

        // 0: left, 1: right, 2: center
        if (step == 0) yaw = -SCAN_STEP_DEGREES;
        else if (step == 1) yaw = +SCAN_STEP_DEGREES;
        else yaw = 0f;

        scanTargetRot = Quaternion.Euler(0f, controller.transform.eulerAngles.y + yaw, 0f);
    }

    // ---- Hide spot checks ----
    private void TickHideSpotChecks()
    {
        if (plan == null || plan.Count == 0)
        {
            controller.context.searchComplete = true;
            return;
        }

        if (idx >= plan.Count)
        {
            controller.context.searchComplete = true;
            return;
        }

        var spot = plan[idx];
        if (spot == null)
        {
            idx++;
            return;
        }

        controller.agent.isStopped = false;
        controller.agent.SetDestination(spot.transform.position);

        if (HasArrived(ARRIVAL_THRESHOLD))
        {
            TryCheckHideSpot(spot);
            idx++; // always advance, even if we decide not to check 
        }
    }
    private void EnsureHideSpotFirst(HideSpot spot)
    {
        if (plan == null) BuildPlan();
        if (spot == null || plan == null) return;

        int i = plan.IndexOf(spot);
        if (i > 0)
        {
            plan.RemoveAt(i);
            plan.Insert(0, spot);
            idx = 0;
        }
        else if (i == 0)
        {
            idx = 0;
        }
    }

    private void BuildPlan()
    {
        plan = new List<HideSpot>();

        Vector3 origin = controller.context.lastKnownPlayerPosition;
        float r2 = SEARCH_RADIUS * SEARCH_RADIUS;

        foreach (var spot in HideSpot.All)
        {
            if (spot == null) continue;

            float d2 = (spot.transform.position - origin).sqrMagnitude;
            if (d2 <= r2)
                plan.Add(spot);
        }

        plan.Sort((a, b) =>
        {
            // If suspicion is on HideSpot:
            int s = b.Suspicion.CompareTo(a.Suspicion);
            if (s != 0) return s;

            float da = (a.transform.position - origin).sqrMagnitude;
            float db = (b.transform.position - origin).sqrMagnitude;
            return da.CompareTo(db);
        });
    }

    private void TryCheckHideSpot(HideSpot spot)
    {
        // Decide whether to check (probability based on suspicion)
        float t = Mathf.Clamp01(spot.Suspicion / SUSPICION_MAX);
        float p = Mathf.Lerp(MIN_CHECK_P, MAX_CHECK_P, t * t);

        // Optional fairness boost if it's the actual player hide spot
        if (spot == controller.context.playerHideSpot) p = Mathf.Min(1f, p + 0.10f);

        bool willCheck = Random.value <= p;

        if (!willCheck)
        {
            // Not checked; tiny decay so ignored spots cool off
            spot.DecreaseSuspicion(0.25f);
            return;
        }

        // Checked:
        if (spot == controller.context.playerHideSpot && spot.IsPlayerInside())
        {
            spot.ForceExit();
            spot.IncreaseSuspicion(2f);

            controller.context.lastTimePlayerSeen = Time.time;
            controller.context.lastKnownPlayerPosition = controller.player.position;
        }
        else
        {
            spot.DecreaseSuspicion(1f);
        }
    }

    private bool HasArrived(float threshold)
    {
        if (controller.agent == null) return false;
        if (controller.agent.pathPending) return false;

        // remainingDistance becomes valid after SetDestination + path computed
        if (controller.agent.remainingDistance > threshold) return false;

        // also handle "no path" or very low velocity
        if (!controller.agent.hasPath || controller.agent.velocity.sqrMagnitude < 0.01f)
            return true;

        return true;
    }
}