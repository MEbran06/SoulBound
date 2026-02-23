using System.Collections.Generic;
using UnityEngine;

public class HallucinationDirector : MonoBehaviour
{
    [Header("Beat Catalog")]
    public List<HallucinationBeat> beats = new();

    [Header("Pacing")]
    public float warmupMin = 3f;
    public float warmupMax = 6f;

    public Vector2 intervalAtLow = new Vector2(5f, 7f); // intensity ~0
    public Vector2 intervalAtHigh = new Vector2(1f, 3f); // intensity ~1

    [Header("Intensity Curve")]
    [Range(1f, 3f)] public float intensityExponent = 1.6f;

    private GhostController controller;

    private float nextBeatTime;
    private readonly Dictionary<string, float> cooldownUntilById = new();
    private string lastBeatId;

    // some helpers for the effects
    public Transform Player => controller.player;
    public Transform Ghost => controller.transform;

    [HideInInspector]
    public Transform PlayerCamera;

    public void Begin(GhostController ghost)
    {
        controller = ghost;
        cooldownUntilById.Clear();
        lastBeatId = null;
        PlayerCamera = Camera.main.transform;

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

        // Convert insanity 100-0 -> intensity 0-1
        float intensity = Mathf.Clamp01(1-insanity01);
        intensity = Mathf.Pow(intensity, intensityExponent);

        // Build eligible list of beats we can play
        float totalWeight = 0f;
        int eligibleCount = 0;

        // We do a two-pass minimal approach: first compute totalWeight,
        // then roll and pick in second pass.
        for (int i = 0; i < beats.Count; i++)
        {
            var b = beats[i];
            if (b == null) continue;
            if (intensity < b.unlockIntensity) continue;

            // check if beat is not on cooldown
            if (cooldownUntilById.TryGetValue(b.id, out float cdUntil) && now < cdUntil)
                continue;

            // Natural preference for lower unlockIntensity beats:
            // delta grows larger for early beats as intensity rises
            float delta = intensity - b.unlockIntensity; // >= 0 here
            float f = 0.2f + Mathf.Max(0f, delta * b.dominanceBias);   // <-- floor
            float w = b.baseWeight * f;

            // avoid degenerate zero weights
            if (w <= 0f) continue;

            totalWeight += w;
            eligibleCount++;
        }

        // if nothing was elegible, just pick the one that is closest to exit cooldown
        if (eligibleCount == 0 || totalWeight <= 0f)
        {
            float soonest = float.PositiveInfinity;

            for (int i = 0; i < beats.Count; i++)
            {
                var b = beats[i];
                if (b == null) continue;
                if (intensity < b.unlockIntensity) continue; // still locked, ignore

                if (cooldownUntilById.TryGetValue(b.id, out float cdUntil))
                    soonest = Mathf.Min(soonest, cdUntil);
                else
                    soonest = now; // no cooldown entry means it's eligible immediately
            }

            if (float.IsPositiveInfinity(soonest))
            {
                // nothing unlocked yet
                nextBeatTime = now + Random.Range(1f, 2f);
            }
            else
            {
                // schedule right when something becomes available
                nextBeatTime = Mathf.Max(now + 0.1f, soonest + Random.Range(0.1f, 0.3f));
            }
            return;
        }

        // Randomly choose a beat, use weight to make randomness bias
        string chosenId = PickWeighted(now, intensity, totalWeight);

        if (!string.IsNullOrEmpty(lastBeatId) && chosenId == lastBeatId && eligibleCount > 1)
        {
            // reroll once (cheap anti-repeat)
            chosenId = PickWeighted(now, intensity, totalWeight);
        }

        // find beat by chosen ID
        var chosenBeat = beats.Find(b => b != null && b.id == chosenId);
        if (chosenBeat != null)
        {
            // execute the beat
            PlayBeat(chosenBeat, intensity);

            // Cooldown bookkeeping
            cooldownUntilById[chosenBeat.id] = now + chosenBeat.cooldown;
            lastBeatId = chosenBeat.id;
        }

        // Schedule next beat based on intensity, make the scheduled time random
        Vector2 window = GetIntervalWindow(intensity);
        nextBeatTime = now + Random.Range(window.x, window.y);
    }

    private string PickWeighted(float now, float intensity, float totalWeight)
    {
        // get a random roll value with the weight
        float roll = Random.value * totalWeight;

        for (int i = 0; i < beats.Count; i++)
        {
            var b = beats[i];
            // beat exists and its unlocked intensity is bellow current intensity
            if (b == null) continue;
            if (intensity < b.unlockIntensity) continue;

            // check that the picked beat is not on cooldown
            if (cooldownUntilById.TryGetValue(b.id, out float cdUntil) && now < cdUntil)
                continue;

            // calculate the change between the current beat and intensity
            float delta = intensity - b.unlockIntensity;

            // effective weight of the beat: used to determine order (lower intensity beats naturally dominate)
            float f = 0.2f + delta; // floor so newly unlocked beats matter
            float w = b.baseWeight * f * b.dominanceBias;
            if (w <= 0f) continue;

            roll -= w;
            if (roll <= 0f)
                return b.id;
        }

        // Fallback (should be rare due to float issues)
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

    protected virtual void PlayBeat(HallucinationBeat beat, float intensity)
    {
        beat.effect?.Play(this, intensity);
    }

    // helper function
    public void Play3DAudio(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null) return;

        AudioSource.PlayClipAtPoint(clip, position, volume);
    }
}