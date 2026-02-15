/*
    This script makes the dot get larger or smaller depending
    on the distance the player is on. It's simply a visualization effect.
*/
using UnityEngine;
using UnityEngine.InputSystem.iOS;

public class DistanceScalerUI : MonoBehaviour
{
    public Transform player;
    public float minScale = 0.5f;
    public float maxScale = 1.2f;
    private float maxDistance;

    Vector3 originalScale;

    void Start()
    {
        // get our scale and the range the player can interat with
        originalScale = transform.localScale;
        maxDistance = player.GetComponent<PlayerInteraction>().interactRange;
    }

    void Update()
    {
        if (player == null) return;
        // calculate distance between the dot and the player
        float dist = Vector3.Distance(player.position, transform.position);

        // divide that distance by the max distance the player can interact with (clamp it between 0 to 1)
        float t = Mathf.Clamp01(dist / maxDistance);
        
        // smoothly transition from one scale to another
        float scale = Mathf.Lerp(maxScale, minScale, t);
        transform.localScale = originalScale * scale;
    }
}
