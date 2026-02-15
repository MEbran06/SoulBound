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
    [SerializeField] TextMeshPro promptText;

    public void ShowPrompt()
    {
        promptText.text = promptMessage;
        promptText.enabled = true;
    }

    public void HidePrompt()
    {
        promptText.enabled = false;
    }

    public KeyCode GetKeyToPress()
    {
        return keyToPress;
    }

    public abstract void Interact(PlayerController player);
}
