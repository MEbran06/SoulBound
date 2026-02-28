using UnityEngine;
using AI.Ghosts.States;
using Items.Ghosts;
using Ghosts.Emotions;

public class StunnedState : GhostState
{
    float timer;
    float decayPerSecond;

    // Expose finished flag for optional debugging, but controller doesn't need it
    public bool IsFinished => timer <= 0f;

    public StunnedState(GhostController controller) : base(controller) { }

    public override void Enter()
    {
        controller.StopMoving();

        // Compute duration and decay from personality
        float startingConfusion = controller.context.emotion.GetEmotion(EmotionType.Confusion);
        float threshold = controller.personality.GetThreshold(EmotionType.Confusion);

        timer = controller.personality.CalculateEmotionDuration(controller, EmotionType.Confusion);
        decayPerSecond = Mathf.Abs(startingConfusion - threshold) / timer;
    }

    public override void Execute()
    {
        controller.StopMoving();

        // decay confusion toward threshold with difficulty multiplier
        float mult = controller.context.difficulty.Get(DifficultyChannel.ConfusionDecay);
        controller.context.emotion.AddFromAI(EmotionType.Confusion, -decayPerSecond * Time.deltaTime, mult);
    }

    public override void Exit() 
    {
        // boost up the aggression by a random amount
        float boost = Random.Range(0f, 5f);
        controller.context.emotion.AddFromAI(EmotionType.Aggression, boost);
    }
}