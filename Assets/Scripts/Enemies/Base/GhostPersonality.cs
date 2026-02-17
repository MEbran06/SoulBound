using GhostStates;
using UnityEngine;

[CreateAssetMenu(fileName = "GhostPersonality", menuName = "Scriptable Objects/GhostPersonality")]
public abstract class GhostPersonality : ScriptableObject
{
    public abstract GhostStateID DecideNextState(GhostController controller);
}
