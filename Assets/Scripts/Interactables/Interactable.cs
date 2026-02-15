/*
    Base class that defines an interactable object
*/

using System;
using TMPro;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    protected string promptMessage = "Press E to (blank)";
    protected KeyCode keyToPress = KeyCode.E;
    [SerializeField] TextMeshProUGUI promptText;
    [SerializeField] public FaderUI dotFader;
    [SerializeField] float dotDistance = 3f;


    // show the prompt and hide the dot
    public void ShowPrompt()
    {
        promptText.enabled = true;
        promptText.text = promptMessage;

        if (dotFader != null)
            dotFader.Hide();
    }



    // hide the prompt and show the dot
    public void HidePrompt()
    {
        promptText.enabled = false;

        if (dotFader != null)
            dotFader.Show();
    }



    public KeyCode GetKeyToPress()
    {
        return keyToPress;
    }
    public float GetDotDistance()
    {
        return dotDistance;
    }

    // this method manages the visibility/invisibility of the dot
   public void UpdateDot(Transform player)
    {
        if (dotFader == null) return;

        float dist = Vector3.Distance(player.position, transform.position);
        // show the prompt if our distance is less than the dot distance
        if (dist <= dotDistance)
            ShowPrompt();
        else
            HidePrompt();
    }



    // abstract method to be implemented by any interactable object
    public abstract void Interact(PlayerController player);
}
