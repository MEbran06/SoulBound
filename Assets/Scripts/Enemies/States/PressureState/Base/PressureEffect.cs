using UnityEngine;

[CreateAssetMenu(fileName = "PressureEffect", menuName = "Scriptable Objects/PressureEffect")]
public abstract class PressureEffect : ScriptableObject
{
    public abstract void Play(PressureDirector director, float intensity);
}
