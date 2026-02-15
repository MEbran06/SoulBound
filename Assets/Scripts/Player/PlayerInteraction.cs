using UnityEngine;
using TMPro;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] public float interactRange = 10f;
    // all interactables are in the interactable layer
    [SerializeField] LayerMask interactLayer;
    [SerializeField] PlayerController player;
    [SerializeField] private GameObject exitUI;


    private Interactable currentInteractable;

    void Update()
    {

        // ---------- Hidden Mode ----------
        if (player.isHidden)
        {
            // prompt player to press E to exit
            exitUI.SetActive(true);

            // call Interact method if key is pressed
            if (currentInteractable != null &&
                Input.GetKeyDown(currentInteractable.GetKeyToPress()))
            {
                currentInteractable.Interact(player);
            }

            return;
        }

        // ---------- Normal Mode ----------
        exitUI.SetActive(false);

        // check if the player is seeing an interactable object
        DetectInteractable();

        // interact if key is pressed
        if (currentInteractable != null &&
            Input.GetKeyDown(currentInteractable.GetKeyToPress()))
        {
            // only allow the player to interact if we're close enough
            float dist = Vector3.Distance(transform.position, currentInteractable.transform.position);
            if (dist <= currentInteractable.GetDotDistance())
            {
                currentInteractable.Interact(player);   
            }
        }
    }



    void DetectInteractable()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;
        // shoot a ray, if we received somthing, it must be interactable 
        if (Physics.Raycast(ray, out hit, interactRange, interactLayer))
        {
            currentInteractable = hit.collider.GetComponent<Interactable>();

            if (currentInteractable != null)
            {
                // show dot or prompt accordingly
                currentInteractable.dotFader.img.enabled = true;
                currentInteractable.UpdateDot(transform);
                return;
            }
        }

        ClearInteractable();
    }


    // clear the image if we're out of range
    void ClearInteractable()
    {
        if (currentInteractable != null)
        {
            currentInteractable.HidePrompt();
            currentInteractable.dotFader.img.enabled = false;
            currentInteractable = null;
        }

    }
}
