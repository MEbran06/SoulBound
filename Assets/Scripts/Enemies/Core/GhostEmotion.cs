using System.Collections.Generic;
using System;
using UnityEngine;

namespace Ghosts.Emotions
{
    public enum EmotionType { Aggression, Confusion, Fear, Attachment }
    public enum EmotionSource { AIState, Personality, Item, Debug }

    [System.Serializable] 
    public struct EmotionValues 
    { 
        public EmotionType emotion; 
        [Range(0f, 100f)] public float value; 
    }

    public struct EmotionTrace
    {
        public EmotionSource source;
        public float delta;
        public float time;
    }

    public class GhostEmotion
    {
        private readonly Dictionary<EmotionType, float> emotionStates;
        private readonly Dictionary<EmotionType, EmotionTrace> lastTrace;

        public GhostEmotion()
        {
            emotionStates = new Dictionary<EmotionType, float>();
            lastTrace = new Dictionary<EmotionType, EmotionTrace>();

            foreach (EmotionType e in Enum.GetValues(typeof(EmotionType)))
            {
                emotionStates[e] = 0f;
                lastTrace[e] = new EmotionTrace { source = EmotionSource.Debug, delta = 0f, time = -Mathf.Infinity };
            }
        }

        // ---- Single choke point ----
        public void AddEmotionDelta(EmotionType emotion, float delta, EmotionSource source)
        {
            ApplyDeltaClamped(emotion, delta);

            // trace emotion changes
            lastTrace[emotion] = new EmotionTrace
            {
                source = source,
                delta = delta,
                time = Time.time
            };
        }

        // Convenience wrappers (prevents call-site chaos)
        public void AddFromAI(EmotionType emotion, float delta, float multiplier = 1f) =>
            AddEmotionDelta(emotion, delta * multiplier, EmotionSource.AIState);

        public void AddFromPersonality(EmotionType emotion, float delta) =>
            AddEmotionDelta(emotion, delta, EmotionSource.Personality);

        // Item deltas should apply sensitivity here, not at call sites
        public void AddFromItem(EmotionType emotion, float rawDelta, float sensitivityMultiplier = 1f) =>
            AddEmotionDelta(emotion, rawDelta * sensitivityMultiplier, EmotionSource.Item);

        public float GetEmotion(EmotionType emotion) => emotionStates[emotion];
        public float Get01(EmotionType emotion) => emotionStates[emotion] / 100f;

        public EmotionTrace GetLastTrace(EmotionType emotion) => lastTrace[emotion];

        // Only use for initialization / save-load / debug
        public void SetEmotionRaw(EmotionType emotion, float value)
        {
            emotionStates[emotion] = Mathf.Clamp(value, 0f, 100f);
            lastTrace[emotion] = new EmotionTrace { source = EmotionSource.Debug, delta = 0f, time = Time.time };
        }

        private void ApplyDeltaClamped(EmotionType emotion, float delta)
        {
            emotionStates[emotion] = Mathf.Clamp(emotionStates[emotion] + delta, 0f, 100f);
        }
    }
}
