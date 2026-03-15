using UnityEngine;

public class GhostMirror : MonoBehaviour
{
    [SerializeField] Transform ghostTransform;
    [SerializeField] Transform player;

    [SerializeField] float maxTurnAngle = 20f;   // max degrees the ghost can deviate
    [SerializeField] float turnSpeed = 60f;      // degrees per second
    [SerializeField] float snapAngle = 2f;

    Quaternion baseRotation;

    void Start()
    {
        // Store the ghost's default orientation
        baseRotation = ghostTransform.rotation;
    }

    void Update()
    {
        Vector3 direction = player.position - ghostTransform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Compute angle difference from base orientation
        float angleFromBase = Quaternion.Angle(baseRotation, targetRotation);

        // Clamp the rotation if it exceeds maxTurnAngle
        if (angleFromBase > maxTurnAngle)
        {
            float t = maxTurnAngle / angleFromBase;
            targetRotation = Quaternion.Slerp(baseRotation, targetRotation, t);
        }

        float remainingAngle = Quaternion.Angle(ghostTransform.rotation, targetRotation);

        if (remainingAngle <= snapAngle)
        {
            ghostTransform.rotation = targetRotation;
        }
        else
        {
            ghostTransform.rotation = Quaternion.RotateTowards(
                ghostTransform.rotation,
                targetRotation,
                turnSpeed * Time.deltaTime
            );
        }
    }
}