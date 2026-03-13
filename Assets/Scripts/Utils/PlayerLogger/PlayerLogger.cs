using UnityEngine;

public class PlayerLogger : MonoBehaviour
{
    public void EncounteredMom()
    {
        MetricsLogger.Instance?.RegisterMomGhostEncounter();
    }

    public void EncounteredDad()
    {
        MetricsLogger.Instance?.RegisterDadGhostEncounter();
    }

    public void EnteredHallucination()
    {
        MetricsLogger.Instance?.RegisterMomHallucination();
    }
}
