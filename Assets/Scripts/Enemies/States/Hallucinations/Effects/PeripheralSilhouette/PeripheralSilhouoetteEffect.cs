using UnityEngine;

[CreateAssetMenu(menuName = "Hallucination/Peripheral Silhouette Effect")]
public class PeripheralSilhouetteEffect : HallucinationEffect
{
    public GameObject prefab;

    [Header("Placement")]
    public float forwardDistance = 6f;
    public float sideOffset = 2f;
    public float verticalOffset = 0f;     // 0 if you snap to ground
    public float lifetime = 1f;

    [Header("Grounding")]
    public LayerMask groundMask;
    public float groundRayHeight = 3f;
    public float groundRayDistance = 10f;

    [Header("Wall and Environment")]
    public LayerMask environmentMask; // walls + props
    public float clearanceRadius = 0.4f;
    public float clearanceHeight = 1.0f;
    public float wallBackoff = 0.75f;

    public override void Play(HallucinationDirector director, float intensity)
    {
        var cam = director.PlayerCamera;
        if (cam == null || prefab == null) return;
        Vector3 pos = GetVisibleSpawnPoint(cam);
        Vector3 desired = cam.position - pos;
        desired.y = 0;
        GameObject obj = Object.Instantiate(prefab, pos, Quaternion.LookRotation(desired));
        Object.Destroy(obj, lifetime);
    }

    Vector3 GetVisibleSpawnPoint(Transform cam)
    {
        float side = Random.value < 0.5f ? -1f : 1f;

        Vector3 desired =
            cam.position +
            cam.forward * forwardDistance +
            cam.right * (sideOffset * side);

        // 1) wall avoid: camera -> desired
        Vector3 camPos = cam.position;
        Vector3 toDesired = desired - camPos;
        float dist = toDesired.magnitude;
        Vector3 dir = toDesired / Mathf.Max(dist, 0.001f);

        if (Physics.Raycast(camPos, dir, out var wallHit, dist, environmentMask, QueryTriggerInteraction.Ignore))
            desired = wallHit.point - dir * wallBackoff;

        // 2) snap to ground at final XZ
        Vector3 rayStart = new Vector3(desired.x, cam.position.y + 10f, desired.z);
        if (Physics.Raycast(rayStart, Vector3.down, out var groundHit, 50f, groundMask, QueryTriggerInteraction.Ignore))
            desired.y = groundHit.point.y;

        // 3) clearance check (don’t spawn inside props)
        Vector3 clearanceCenter = desired + Vector3.up * clearanceHeight;
        if (Physics.CheckSphere(clearanceCenter, clearanceRadius, environmentMask, QueryTriggerInteraction.Ignore))
        {
            // simple fallback: move closer to camera a bit
            desired -= cam.forward * 1.5f;
        }

        return desired;
    }
}