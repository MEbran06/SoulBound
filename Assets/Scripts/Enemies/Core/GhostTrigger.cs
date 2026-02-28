using UnityEngine;

public class GhostTrigger : MonoBehaviour
{
    [SerializeField] GhostPersonality ghostPersona; 
    [SerializeField] GhostController controller;

    private void OnTriggerEnter(Collider other)
    {
        if (ghostPersona != null && other.CompareTag("Player"))
        {
            // Call a function in the ScriptableObject to handle the logic
            ghostPersona.HandleTriggerEnter(other, controller);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ghostPersona.HandleTriggerExit(other, controller);
    }
}
