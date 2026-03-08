using UnityEngine;

public class GhostHearing : MonoBehaviour
{
    public GhostController controller;

    [Header("Hearing")]
    public float hearingRadiusAtLoudness1 = 16f;   // loudness=1 reaches this far
    public float noiseMemorySeconds = 4f;          // how long Patrol can react to a sound
    public float minLoudnessToCare = 0.10f;
    public float loudnessMargin = 0.05f;
    [Header("Escalation")]
    public float urgentHearingThreshold = 0.55f;
    public float urgentChaseRadius = 6f;

    public LayerMask occlusionMask;
    public float occlusionPenalty = 0.55f; // reduces loudness if blocked

    void Reset()
    {
        controller = GetComponentInParent<GhostController>();
    }

    void OnEnable()
    {
        NoiseSystem.OnNoise += OnNoiseHeard;
    }

    void OnDisable()
    {
        NoiseSystem.OnNoise -= OnNoiseHeard;
    }

    void OnNoiseHeard(Vector3 pos, float loudness)
    {
        if (!controller) return;
        if (loudness < minLoudnessToCare) return;

        // Distance falloff: audible if within loudness-scaled radius
        float radius = hearingRadiusAtLoudness1 * loudness;
        float d2 = (pos - controller.transform.position).sqrMagnitude;
        if (d2 > radius * radius) return;

        float effective = loudness;

        // reduce effective noise if there are walls or objects in between
        Vector3 from = controller.transform.position + Vector3.up * 1.6f;
        Vector3 to = pos + Vector3.up * 1.0f;
        if (Physics.Linecast(from, to, occlusionMask))
            effective *= occlusionPenalty;


        // Keep the strongest recent noise (prevents tiny step noise overwriting a slam)
        float currentAge = Time.time - controller.context.lastHeardTime;
        bool currentExpired = currentAge > noiseMemorySeconds;

        if (currentExpired || effective >= controller.context.lastHeardLoudness + loudnessMargin)
        {
            controller.context.lastHeardPosition = pos;
            controller.context.lastHeardTime = Time.time;
            controller.context.lastHeardLoudness = effective;

            float dist = Vector3.Distance(controller.transform.position, pos);
            controller.context.lastHeardWasUrgent =
                effective >= urgentHearingThreshold || dist <= urgentChaseRadius;
        }

        controller.context.heardNoiseThisFrame = true;
    }

    void LateUpdate()
    {
        if (controller) controller.context.heardNoiseThisFrame = false;
    }
}
