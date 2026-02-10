using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float xRot = 0;
    [SerializeField] float mouseSensibility = 100;
    [SerializeField] Transform player;

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensibility * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensibility * Time.deltaTime;
        xRot -= mouseY;

        // avoid the player to keep turning indefinetely 
        xRot = Mathf.Clamp(xRot, -20, 70);

        // take the transform of the current object and rotate it
        transform.localRotation = Quaternion.Euler(xRot, 0f, 0f);
        // Rotate the player right or left
        player.Rotate(Vector3.up * mouseX);

    }
}
