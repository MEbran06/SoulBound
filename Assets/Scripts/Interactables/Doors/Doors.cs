using UnityEngine;
using System.Collections;

public class Doors : Interactable
{
    [Header("Door Settings")]
    [SerializeField] private Transform doorPivot;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 120f;
    [SerializeField] private bool startsOpen = false;

    private bool isOpen;
    private bool isMoving;

    private Quaternion closedRotation;
    private Quaternion openedRotation;

    private void Start()
    {
        promptMessage = "Press E to Open";

        if (doorPivot == null)
            doorPivot = transform;

        closedRotation = doorPivot.localRotation;
        openedRotation = closedRotation * Quaternion.Euler(0f, openAngle, 0f);

        isOpen = startsOpen;

        if (isOpen)
            doorPivot.localRotation = openedRotation;
        else
            doorPivot.localRotation = closedRotation;
    }

    public override void Interact(PlayerController player)
    {
        if (isMoving) return;

        StartCoroutine(RotateDoor(isOpen ? closedRotation : openedRotation));
        isOpen = !isOpen;
        // Update prompt
        promptMessage = isOpen ? "Press E to Close" : "Press E to Open";
    }

    private IEnumerator RotateDoor(Quaternion targetRotation)
    {
        isMoving = true;

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

    public bool IsOpen() => isOpen;
    public bool IsMoving() => isMoving;
}