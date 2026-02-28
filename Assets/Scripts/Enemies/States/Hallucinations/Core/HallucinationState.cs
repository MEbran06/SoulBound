using UnityEngine;
using Ghosts.Emotions;
public class HallucinationState : GhostState
{
    private HallucinationDirector director;

    public HallucinationState(GhostController controller) : base(controller) { }

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
        // drive the mom's aggressing up as sanity of the player goes down
        float mult = controller.context.difficulty.Get(DifficultyChannel.AggressionRate);
        controller.context.emotion.AddFromAI(EmotionType.Aggression, buildUpRate*Time.deltaTime, mult);

        // tick expects insanity normalized
        director.Tick(insanity/ controller.context.insanitySystem.maxInsanity);
    }

    public override void Exit() 
    {
        director?.End();
        director = null;
        // appear close to the player
        controller.GetVisibleSpawnPoint(Camera.main.transform);
        controller.SetVisible(true);
    }
}