using AI.Ghosts.States;
using Items.Ghosts;
using UnityEngine;

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
    public EmotionSensitivity[] sensitivities;
    
    [Header("Initial Emotions")]
    public EmotionValues[] startingEmotions;

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
}
