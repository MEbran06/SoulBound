using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController characterController;
    [SerializeField] float speed = 4f;
    private float x, z;
    float gravity = -20f;
    float verticalVelocity;
    private Vector3 move;
    public bool isHidden = false;

    private float standingHeight;
    private bool isCrouching = false;
    private Camera playerCamera; // Reference to the player camera
    private Vector3 cameraPosition;

    [Header("Stamina")]
    [SerializeField] float maxStamina = 5f;
    [SerializeField] float staminaDrainRate = 1f;      // per second while sprinting
    [SerializeField] float staminaRegenRate = 0.75f;   // per second while resting
    [SerializeField] float sprintThreshold = 2f;
    private bool canSprint = true;

    private float currentStamina;

    // Basic Inventory -> to be replaced by actual inventory system
    public Transform handTransform;
    private List<Item> inventory = new List<Item>();
    private Item currentHeldItem;


    void Start()
    {
        // player camera is the main camera
        playerCamera = Camera.main;
        cameraPosition = playerCamera.transform.localPosition;

        // store the current height of the character
        standingHeight = characterController.height;

        // initaialize stamina
        currentStamina = maxStamina;

    }

    // Update is called once per frame
    void Update()
    { 
        // disable movement while hidden
        if (isHidden) return;

        bool isGrounded = characterController.isGrounded;
        float currentSpeed = speed;
        bool sprintKey = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (Input.GetKeyDown(KeyCode.C)) // Use 'C' key for toggle
        {
            isCrouching = !isCrouching;
            Crouch();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            DropCurrentItem();
        }

        // left click on mouse to use item
        if (Input.GetMouseButtonDown(0))
        {
            currentHeldItem?.Use();
        }

        if (currentStamina <= 0f)
            canSprint = false;

        if (currentStamina >= sprintThreshold)
            canSprint = true;

        // crouch modifier
        if (isCrouching)
        {
            currentSpeed *= 0.5f;
        }

        if (sprintKey && currentStamina > 0f && canSprint && !isCrouching)
        {
            currentSpeed *= 2f;
            currentStamina -= staminaDrainRate * Time.deltaTime;
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
        }


        // clamp stamina between 0 to max stamina value
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        if (isGrounded && verticalVelocity < 0) {
            verticalVelocity = -2f; // Keep grounded
        }

        verticalVelocity += gravity * Time.deltaTime;
        // get x and y position of the player
       x = Input.GetAxis("Horizontal");
       z = Input.GetAxis("Vertical");

        // move forward/back and left/right
        move = (transform.right * x + transform.forward * z) * currentSpeed;
        move.y = verticalVelocity;

        // move a certain position at some speed
       characterController.Move(move*Time.deltaTime);
        
    }

    void Crouch()
    {
        if (isCrouching)
        {
            // crouch height is half our standing height
            characterController.height = standingHeight / 2f;
            // Adjust camera position down by half the difference in height
            if (playerCamera != null)
            {
                playerCamera.transform.localPosition = new Vector3(cameraPosition.x, cameraPosition.y/2f, cameraPosition.z);
            }
        }
        else
        {
            characterController.height = standingHeight;
            // Adjust camera position up
            if (playerCamera != null)
            {
                playerCamera.transform.localPosition = cameraPosition;
            }
        }
    }

    public void AddToInventory(Item item)
    {
        inventory.Add(item);
    }

    public void HoldItem(Item item)
    {
        if (currentHeldItem != null)
            DropCurrentItem();

        currentHeldItem = item;

        item.transform.SetParent(handTransform);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;

        item.OnHeld();
    }

    public void DropCurrentItem()
    {
        if (currentHeldItem == null)
            return;

        currentHeldItem.OnDropped();
        currentHeldItem.transform.SetParent(null);
        currentHeldItem = null;
    }
}
