using UnityEngine;

[CreateAssetMenu(menuName = "Pressure/WhisperEffect")]
public class WhisperEffect : PressureEffect
{
    public AudioClip[] AudioLines;

    public float distanceBehindPlayer = 2f;

    public override void Play(PressureDirector director, float intensity)
    {
        Transform player = director.Player;

        Vector3 pos = player.position - player.forward * distanceBehindPlayer;

        AudioClip clip;

        clip = AudioLines[Random.Range(0, AudioLines.Length)];

        director.Play3DAudio(clip, pos, Mathf.Lerp(0.4f, 1f, intensity));
    }
}