using UnityEngine;

[System.Serializable]
public class HallucinationBeat
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

    // beat behavior
    public HallucinationEffect effect;
}
