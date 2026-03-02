using System;
using UnityEngine;

public static class NoiseSystem
{
    // loudness 0..1
    public static event Action<Vector3, float> OnNoise;

    public static void Emit(Vector3 pos, float loudness)
    {
        if (loudness <= 0f) return;
        OnNoise?.Invoke(pos, loudness);
    }
}
