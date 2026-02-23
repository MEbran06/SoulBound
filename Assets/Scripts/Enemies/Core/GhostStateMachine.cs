using AI.Ghosts.States;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
// class for handling switching states
public class GhostStateMachine
{

    public GhostController controller;
    public  IGhostState CurrentState { get; private set; }
    public GhostStateID CurrentStateID {get; private set; }
    private Dictionary<GhostStateID, IGhostState> cachedStates = new Dictionary<GhostStateID, IGhostState>();


    public GhostStateMachine(GhostController control)
    {
        controller = control;
    }

    public void ChangeState(GhostStateID newState)
    {
        CurrentState?.Exit();
        CurrentState = GetGhostState(newState);
        CurrentStateID = newState;
        CurrentState?.Enter();
    }

    public IGhostState GetGhostState(GhostStateID state)
    {
        IGhostState ghostState = null;
        if (cachedStates.ContainsKey(state))
        {
            // returned the cached state if it's already created
            ghostState = cachedStates[state];
        }
        else
        {
            // factory for creating new states
            ghostState = CreateGhostState(state);
            // save it for later
            cachedStates.Add(state, ghostState); 
        }
        // may return null
        return ghostState;
    }

    // factory method for state machine
    // Must update this when creating new states
    public IGhostState CreateGhostState(GhostStateID state)
    {
        switch (state)
        {
            case GhostStateID.Patrol:
                return new PatrolState(controller);
            case GhostStateID.Chase:
                return new ChaseState(controller);
            case GhostStateID.Stunned:
                return new StunnedState(controller);
            case GhostStateID.Hallucination:
                return new HallucinationState(controller);
            case GhostStateID.Stalk:
                return new StalkState(controller);
            case GhostStateID.ManifestAttack:
                return new ManifestAttackState(controller);
        }

        return null;
    }

    public void Update()
    {
        CurrentState?.Execute();
    }
}

