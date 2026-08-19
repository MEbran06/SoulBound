using UnityEngine;
using Ghosts.Emotions;

public class PressureState : GhostState
{
    private PressureDirector director;

    public PressureState(GhostController controller) : base(controller) { }

    public override void Enter()
    {
        controller.SetVisible(false);

        director = controller.director;
        director?.Begin(controller);
    }

    public override void Execute()
    {
        if (director == null) return;

        float insanity = controller.context.insanitySystem.CurrentInsanity;
        float buildUpRate = controller.personality.aggressionBuildUpRate;

        float mult = controller.context.difficulty.Get(DifficultyChannel.AggressionRate);
        controller.context.emotion.AddFromAI(
            EmotionType.Aggression,
            buildUpRate * Time.deltaTime,
            mult
        );

        director.Tick(insanity / controller.context.insanitySystem.maxInsanity);
    }

    public override void Exit()
    {
        director?.End();
        director = null;

        // Pressure is now Mom's default state.
        controller.SetVisible(true);
    }
}