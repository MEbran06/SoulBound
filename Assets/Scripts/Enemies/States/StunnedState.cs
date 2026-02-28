using UnityEngine;
using AI.Ghosts.States;
using Items.Ghosts;

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
        float startingConfusion = controller.context.GetEmotion(EmotionType.Confusion);
        float threshold = controller.personality.GetThreshold(EmotionType.Confusion);

        timer = controller.personality.CalculateEmotionDuration(controller, EmotionType.Confusion);
        decayPerSecond = Mathf.Abs(startingConfusion - threshold) / timer;
    }

    public override void Execute()
    {
        controller.StopMoving();

        // decay confusion toward threshold
        controller.context.ModifyEmotion(EmotionType.Confusion, -decayPerSecond * Time.deltaTime);
    }

    public override void Exit() 
    {
        // boost up the aggression by a random amount
        float boost = Random.Range(0f, 5f);
        controller.context.ModifyEmotion(EmotionType.Aggression, boost);
    }
}