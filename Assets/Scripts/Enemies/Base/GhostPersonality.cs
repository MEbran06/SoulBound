using AI.Ghosts.States;
using Items.Ghosts;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;

[System.Serializable]
public struct EmotionSensitivity
{
    public EmotionType emotion;
    public float multiplier;
}


[CreateAssetMenu(fileName = "GhostPersonality", menuName = "Scriptable Objects/GhostPersonality")]
public abstract class GhostPersonality : ScriptableObject
{
    // you need to define this for all emotion types
    [Header("Emotion Sensitivities")]
    public EmotionSensitivity[] sensitivities;
    [Header("Emotion Thresholds")]
    public EmotionValues[] thresholds;
    [Header("Initial Emotions")]
    public EmotionValues[] startingEmotions;

    public float aggressionBuildUpRate = 5f;

    // Base duration for reactions
    public float baseDuration = 3f;

    public virtual void InitializeGhost(GhostController controller)
    {
        foreach (var emotion in startingEmotions)
        {
            controller.context.SetEmotion(
                emotion.emotion,
                emotion.value
            );
        }
    }
    public abstract GhostStateID DecideNextState(GhostController controller);

    public abstract void ApplyGhostItemEffect(GhostController controller, GhostItemData data);

    protected float GetSensitivity(EmotionType emotion)
    {
        // sensitivities will be part of the scriptable object, and initialized on the inspector
        foreach (var s in sensitivities)
        {
            if (s.emotion == emotion)
                return s.multiplier;
        }

        return 1f; // default multiplier if not found
    }

    public virtual float CalculateEmotionDuration(GhostController controller, EmotionType emotion)
    {
        // Sensitivity for confusion
        float sensitivity = GetSensitivity(emotion);

        // Scale with item strength and sensitivity
        float scaledDuration = baseDuration * sensitivity;

        return scaledDuration;
    }

    public float GetThreshold(EmotionType emotion)
    {
        foreach (EmotionValues threshold in thresholds)
        {
            if (threshold.emotion == emotion)
                return threshold.value;
        }

        return 0f; // we couldn't find a threshold
    }

    public virtual void HandleTriggerEnter(Collider other, GhostController controller)
    {
        // let children decide what to do
    }
}
