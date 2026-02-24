using UnityEngine;

[CreateAssetMenu(menuName = "Hallucination/Peripheral Silhouette Effect")]
public class PeripheralSilhouetteEffect : HallucinationEffect
{
    public GameObject prefab;
    public float lifetime = 1f;

    public override void Play(HallucinationDirector director, float intensity)
    {
        var cam = director.PlayerCamera;
        if (cam == null || prefab == null) return;
        Vector3 pos = director.controller.GetVisibleSpawnPoint(cam);
        Vector3 desired = cam.position - pos;
        desired.y = 0;
        GameObject obj = Object.Instantiate(prefab, pos, Quaternion.LookRotation(desired));
        Object.Destroy(obj, lifetime);
    }
}