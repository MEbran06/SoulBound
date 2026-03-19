using System.Collections.Generic;
using Ghosts.Emotions;
using UnityEngine;

public class PressureDirector : MonoBehaviour
{
    [Header("Beat Catalog")]
    public List<PressureBeat> beats = new();

    [Header("Pacing")]
    public float warmupMin = 3f;
    public float warmupMax = 6f;

    public Vector2 intervalAtLow = new Vector2(5f, 7f);
    public Vector2 intervalAtHigh = new Vector2(1f, 3f);

    [Header("Intensity Curve")]
    [Range(1f, 3f)] public float intensityExponent = 1.6f;

    [Header("Mirror Query")]
    [SerializeField] float mirrorSearchRadius = 4f;
    [SerializeField] LayerMask mirrorLayer;
    [SerializeField] LayerMask visibilityBlockers = ~0;

    public GhostController controller;

    private float nextBeatTime;
    private readonly Dictionary<string, float> cooldownUntilById = new();
    private string lastBeatId;

    public Transform PlayerCamera;

    public Transform Player => controller.player;
    public Transform Ghost => controller.transform;

    public void Begin(GhostController ghost)
    {
        controller = ghost;
        cooldownUntilById.Clear();
        lastBeatId = null;

        nextBeatTime = Time.time + Random.Range(warmupMin, warmupMax);
    }

    public void End()
    {
        controller = null;
        lastBeatId = null;
    }

    public void Tick(float insanity01)
    {
        if (controller == null) return;
        if (beats == null || beats.Count == 0) return;

        float now = Time.time;
        if (now < nextBeatTime) return;

        float intensity = Mathf.Clamp01(1f - insanity01);
        intensity = Mathf.Pow(intensity, intensityExponent);

        float totalWeight = 0f;
        int eligibleCount = 0;

        for (int i = 0; i < beats.Count; i++)
        {
            var b = beats[i];
            if (!IsBeatEligible(b, now, intensity)) continue;

            totalWeight += ComputeWeight(b, intensity);
            eligibleCount++;
        }

        if (eligibleCount == 0 || totalWeight <= 0f)
        {
            nextBeatTime = now + Random.Range(0.75f, 1.5f);
            return;
        }

        string chosenId = PickWeighted(now, intensity, totalWeight);

        if (!string.IsNullOrEmpty(lastBeatId) && chosenId == lastBeatId && eligibleCount > 1)
            chosenId = PickWeighted(now, intensity, totalWeight);

        var chosenBeat = beats.Find(b => b != null && b.id == chosenId);
        if (chosenBeat != null)
        {
            PlayBeat(chosenBeat, intensity);
            cooldownUntilById[chosenBeat.id] = now + chosenBeat.cooldown;
            lastBeatId = chosenBeat.id;
        }

        Vector2 window = GetIntervalWindow(intensity);
        nextBeatTime = now + Random.Range(window.x, window.y);
    }

    private bool IsBeatEligible(PressureBeat beat, float now, float intensity)
    {
        if (beat == null) return false;
        if (intensity < beat.unlockIntensity) return false;

        if (cooldownUntilById.TryGetValue(beat.id, out float cdUntil) && now < cdUntil)
            return false;

        // Context-aware gating by beat family
        switch (beat.type)
        {
            case PressureBeatType.Mirror:
                return HasVisibleMirror();

            case PressureBeatType.Reveal:
                return Time.time >= controller.context.nextAllowedRevealTime;

            case PressureBeatType.AttackCommit:
                bool remembersPlayer =
                    Time.time < controller.context.lastTimePlayerSeen + controller.rememberPlayerTime;

                bool chasePossible = controller.context.canSeePlayer || remembersPlayer;
                bool vulnerable =
                    controller.context.insanitySystem.CurrentInsanity <=
                    controller.personality.GetThreshold(EmotionType.Fear);

                return chasePossible && vulnerable;

            default:
                return true;
        }
    }

    private float ComputeWeight(PressureBeat beat, float intensity)
    {
        float delta = intensity - beat.unlockIntensity;
        float f = 0.2f + Mathf.Max(0f, delta * beat.dominanceBias);
        return beat.baseWeight * f;
    }

    private string PickWeighted(float now, float intensity, float totalWeight)
    {
        float roll = Random.value * totalWeight;

        for (int i = 0; i < beats.Count; i++)
        {
            var b = beats[i];
            if (!IsBeatEligible(b, now, intensity)) continue;

            float w = ComputeWeight(b, intensity);
            if (w <= 0f) continue;

            roll -= w;
            if (roll <= 0f)
                return b.id;
        }

        for (int i = beats.Count - 1; i >= 0; i--)
        {
            var b = beats[i];
            if (b != null) return b.id;
        }

        return null;
    }

    private Vector2 GetIntervalWindow(float intensity)
    {
        float minI = Mathf.Lerp(intervalAtLow.x, intervalAtHigh.x, intensity);
        float maxI = Mathf.Lerp(intervalAtLow.y, intervalAtHigh.y, intensity);
        if (maxI < minI) maxI = minI + 0.5f;
        return new Vector2(minI, maxI);
    }

    protected virtual void PlayBeat(PressureBeat beat, float intensity)
    {
        beat.effect?.Play(this, intensity);
    }

    public void QueueReveal()
    {
        if (controller == null) return;
        controller.context.pressureRevealQueued = true;
    }

    public void QueueAttackCommit(float commitSeconds = 1.25f)
    {
        if (controller == null) return;
        controller.context.pressureAttackQueued = true;
        controller.context.attackCommittedUntilTime = Mathf.Max(
            controller.context.attackCommittedUntilTime,
            Time.time + commitSeconds
        );
    }

    public void QueueRevealAppearance(float commitSeconds = 1.25f)
    {
        if (controller == null) return;
        controller.context.pressureRevealQueued = true;
        controller.context.nextAllowedRevealTime = Mathf.Max(
            controller.context.nextAllowedRevealTime,
            Time.time + commitSeconds
        );
    }

    public bool HasVisibleMirror()
    {
        return GetVisibleMirror() != null;
    }

    public void Play3DAudio(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, volume);
    }

    public Collider GetVisibleMirror()
    {
        if (Player == null || PlayerCamera == null)
            return null;

        Collider[] hits = Physics.OverlapSphere(
            Player.position,
            mirrorSearchRadius,
            mirrorLayer,
            QueryTriggerInteraction.Collide
        );

        Camera cam = Camera.main;
        if (cam == null)
            return null;

        Collider bestMirror = null;
        float bestScore = float.NegativeInfinity;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider mirror = hits[i];
            if (mirror == null) continue;

            Vector3 mirrorPoint = mirror.bounds.center;
            Vector3 viewport = cam.WorldToViewportPoint(mirrorPoint);

            bool inFront = viewport.z > 0f;
            bool inViewport =
                viewport.x >= 0f && viewport.x <= 1f &&
                viewport.y >= 0f && viewport.y <= 1f;

            if (!inFront || !inViewport)
                continue;

            Vector3 origin = PlayerCamera.position;
            Vector3 dir = mirrorPoint - origin;
            float dist = dir.magnitude;

            if (dist <= 0.001f)
                return mirror;

            dir /= dist;

            if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, visibilityBlockers, QueryTriggerInteraction.Ignore))
            {
                if (!(hit.collider == mirror || hit.collider.transform.IsChildOf(mirror.transform)))
                    continue;
            }

            // Prefer mirrors closer to center of the screen
            float centerScore =
                1f - Vector2.Distance(
                    new Vector2(viewport.x, viewport.y),
                    new Vector2(0.5f, 0.5f)
                );

            float score = centerScore - dist * 0.05f;

            if (score > bestScore)
            {
                bestScore = score;
                bestMirror = mirror;
            }
        }

        return bestMirror;
    }
}