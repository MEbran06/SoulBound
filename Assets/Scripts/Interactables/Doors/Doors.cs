using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class Doors : Interactable
{
    [Header("Door Settings")]
    [SerializeField] private Transform doorPivot;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 120f;
    [SerializeField] private bool startsOpen = false;

    [Header("Lock Settings")]
    [SerializeField] private bool isUnlockable = true;
    [SerializeField] private bool isUnlocked = true;
    [SerializeField] private string requiredKeyId;
    [SerializeField] private bool consumeKeyOnUnlock = false;

    private bool isOpen;
    private bool isMoving;

    private Quaternion closedRotation;
    private Quaternion openedRotation;

    public AudioSource asource;
    public AudioClip openDoor, closeDoor;

    private void Start()
    {
        // promptMessage = "Press E to Open";

        if (doorPivot == null)
            doorPivot = transform;

        closedRotation = doorPivot.localRotation;
        openedRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);

        isOpen = startsOpen;

        if (isOpen)
            doorPivot.localRotation = openedRotation;
        else
            doorPivot.localRotation = closedRotation;

        UpdatePrompt();
    }

    public override void Interact(PlayerController player)
    {
        if (isMoving) return;

        ItemSO heldItem = player.inventoryUI.ItemOnHand;

        if (isUnlockable && !isUnlocked)
        {
            bool hasKey = heldItem.isKey;

            if (!hasKey)
            {
                Debug.Log("Door is locked.");
                PopupMessage.Instance.ShowMessage("The door is locked. I need a key.");
                return;
            }
            else if (!heldItem.keyId.Equals(requiredKeyId))
            {
                Debug.Log("Wrong Key.");
                PopupMessage.Instance.ShowMessage("This is not the right key.");
                return;
            }

            isUnlocked = true;
            
            if (consumeKeyOnUnlock)
                player.inventoryUI.TryConsumeKey(requiredKeyId);

            Debug.Log("Door unlocked.");
        }
     
        StartCoroutine(RotateDoor(isOpen ? closedRotation : openedRotation));
        isOpen = !isOpen;
        // Update prompt
        UpdatePrompt();
    }

    private IEnumerator RotateDoor(Quaternion targetRotation)
    {
        isMoving = true;
        asource.clip = !isOpen ? openDoor : closeDoor;
        asource.Play();
        while (Quaternion.Angle(doorPivot.localRotation, targetRotation) > 0.5f)
        {
            doorPivot.localRotation = Quaternion.RotateTowards(
                doorPivot.localRotation,
                targetRotation,
                openSpeed * Time.deltaTime
            );

            yield return null;
        }
        doorPivot.localRotation = targetRotation;
        isMoving = false;
    }

    private void UpdatePrompt()
    {
        if (isUnlockable && !isUnlocked)
            promptMessage = "Press E to Unlock";
        else
            promptMessage = isOpen ? "Press E to Close" : "Press E to Open";
    }

    public bool IsOpen() => isOpen;
    public bool IsMoving() => isMoving;
}