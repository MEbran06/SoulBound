using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] CharacterController characterController;
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask LayerMaskGround;
    [SerializeField] float speed = 4f;
    [SerializeField] float jumpForce = 4f;
    [SerializeField] float gravity = -20f;
    
    private Vector3 vel;
    private float x, z;
    private Vector3 move;

    // Update is called once per frame
    void Update()
    {
        // get x and y position of the player
       x = Input.GetAxis("Horizontal");
       z = Input.GetAxis("Vertical");

        // move forward/back and left/right
       move = transform.right*x + transform.forward*z;

        // move a certain position at some speed
       characterController.Move(move*speed*Time.deltaTime);
        
        
    }
}
