using GhostStates;
using Unity.VisualScripting;
using UnityEngine;
// class for handling switching states
public class GhostStateMachine
{

    public GhostController controller;
    public  IGhostState CurrentState { get; private set; }
    public GhostStateID CurrentStateID {get; private set; }

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
        if (CachedStates.states.ContainsKey(state))
        {
            // returned the cached state if it's already created
            ghostState = CachedStates.states[state];
        }
        else
        {
            // factory for creating new states
            ghostState = CreateGhostState(state);
            // save it for later
            CachedStates.states.Add(state, ghostState); 
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
        }

        return null;
    }

    public void Update()
    {
        CurrentState?.Execute();
    }
}

