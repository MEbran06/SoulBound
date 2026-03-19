using UnityEngine;

[CreateAssetMenu(menuName = "Pressure/Stalk Effect")]
public class StalkEffect : PressureEffect
{
    public float stalkDuration = 8f;
    public override void Play(PressureDirector director, float intensity)
    {
        //Debug.Log("Entered Stalk effect");
        // schedule the stalk appearance
        director.QueueRevealAppearance(stalkDuration);
    }
}