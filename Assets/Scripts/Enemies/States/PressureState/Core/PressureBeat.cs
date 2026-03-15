using UnityEngine;

public enum PressureBeatType
{
    LightSabotage,
    Mirror,
    Environment,
    Hallucination,
    Reveal,
    AttackCommit
}

[System.Serializable]
public class PressureBeat
{
    public string id;

    [Range(0f, 1f)]
    public float unlockIntensity;

    [Min(0f)]
    public float baseWeight = 1f;

    [Min(0f)]
    public float cooldown = 8f;
    
    // how quickly this beat becomes likely after unlocking
    [Min(0f)]
    public float dominanceBias = 1f;

    // beat type
    public PressureBeatType type;

    // beat behavior
    public PressureEffect effect;
}
