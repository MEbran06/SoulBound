using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] CharacterController characterController;
    [SerializeField] float speed = 4f;
    private float x, z;
    float gravity = -20f;
    float verticalVelocity;
    private Vector3 move;

    // Update is called once per frame
    void Update()
    {
        bool isGrounded = characterController.isGrounded;

        if (isGrounded && verticalVelocity < 0) {
            verticalVelocity = -2f; // Keep grounded
        }
        verticalVelocity += gravity * Time.deltaTime;
        // get x and y position of the player
       x = Input.GetAxis("Horizontal");
       z = Input.GetAxis("Vertical");

        // move forward/back and left/right
       move = (transform.right*x + transform.forward*z)*speed;
       move.y = verticalVelocity;

        // move a certain position at some speed
       characterController.Move(move*Time.deltaTime);
       
        
        
    }
}
