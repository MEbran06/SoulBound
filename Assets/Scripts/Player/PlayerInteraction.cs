using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] float interactRange = 10f;
    [SerializeField] LayerMask interactLayer;
    [SerializeField] PlayerController player;
    [SerializeField] private GameObject exitUI;


    private Interactable currentInteractable;

    void Update()
    {
        if (!player.isHidden)
        {
            DetectInteractable();
            exitUI.SetActive(false);
        }
        else
        {
            exitUI.SetActive(true);

            // Allow exit even without raycast
            if (currentInteractable != null &&
                Input.GetKeyDown(currentInteractable.GetKeyToPress()))
            {
                currentInteractable.Interact(player);
            }

            return;
        }

        if (currentInteractable != null)
        {
            if (Input.GetKeyDown(currentInteractable.GetKeyToPress()))
            {
                currentInteractable.Interact(player);
            }
        }
    }


    void DetectInteractable()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange, interactLayer))
        {
            currentInteractable = hit.collider.GetComponent<Interactable>();

            if (currentInteractable != null)
            {
                currentInteractable.ShowPrompt();
                return;
            }
        }
        ClearInteractable();
    }

    void ClearInteractable()
    {
        if (currentInteractable != null)
        {
            currentInteractable.HidePrompt();
            currentInteractable = null;
        }

    }
}
