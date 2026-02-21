using UnityEngine;

[CreateAssetMenu(fileName = "HallucinationEffect", menuName = "Scriptable Objects/HallucinationEffect")]
public abstract class HallucinationEffect : ScriptableObject
{
    public abstract void Play(HallucinationDirector director, float intensity);
}
