using UnityEngine;

public interface IGhostState
{
    void Enter();
    void Execute();
    void Exit();
}


public abstract class GhostState : IGhostState
{
   protected GhostController controller;

    public GhostState(GhostController gControl)
    {
        controller = gControl;
    }

    public abstract void Enter();
    public abstract void Execute();
    public abstract void Exit();
}

