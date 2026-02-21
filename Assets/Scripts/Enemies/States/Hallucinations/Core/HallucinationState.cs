using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.AI;

public class HallucinationState : GhostState
{
    private HallucinationDirector director;

    public HallucinationState(GhostController controller) : base(controller) { }

    public override void Enter()
    {
        director = controller.director;
        director?.Begin(controller);
    }

    public override void Execute()
    {
        if (director == null) return;

        float insanity = controller.context.insanitySystem.CurrentInsanity;
        // tick expects insanity normalized
        director.Tick(insanity/100f);
    }

    public override void Exit() 
    {
        director?.End();
        director = null;
    }
}