using AI.Ghosts.States;
using Items.Ghosts;
using UnityEngine;
using Ghosts.Emotions;

[System.Serializable]
public struct EmotionSensitivity
{
    public EmotionType emotion;
    public float multiplier;
}

[System.Serializable]
public struct AttachmentDifficultySensitivity
{
    public DifficultyChannel channel;
    // multiplier at 0 attachment
    public float multAt0;
    // multiplier at 100 attachment
    public float multAt100;
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
    [Header("Multiplier effect of Child attachment on the Ghost")]
    public AttachmentDifficultySensitivity[] attachmentDifficulty;

    public float aggressionBuildUpRate = 5f;
    public float aggressionDecayRate = 1.0f;

    // Base duration for reactions
    public float baseDuration = 3f;

    public void InitializeGhost(GhostController controller)
    {
        foreach (var emotion in startingEmotions)
        {
            controller.context.emotion.SetEmotionRaw(
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

    public virtual void ApplyAttachmentDifficulty(GhostContext ctx, float attachment01)
    {
        foreach (var s in attachmentDifficulty)
        {
            // attachment drives the change between multiplier
            float m = Mathf.Lerp(s.multAt0, s.multAt100, attachment01);
            ctx.difficulty.Set(s.channel, m);
        }
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

    public virtual void HandleTriggerExit(Collider other, GhostController controller)
    {
        // let children decide what to do
    }
}