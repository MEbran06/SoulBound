using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    void LateUpdate()
    {
        if (Camera.main == null) return;
        // make the prompt for the interactable object always face the player
        transform.forward = Camera.main.transform.forward;
    }
}
