using UnityEngine;

[CreateAssetMenu(menuName = "Items/Usables/Lantern Toggle")]
public class LanternUsableData : UsableItemData
{
    [SerializeField] public float toggleCooldown = 0.15f;
    private float lastToggleTime = -Mathf.Infinity;

    public override void Use(PlayerController player, Inventory inventory)
    {
        if (player == null) return;
        if (Time.time < lastToggleTime + toggleCooldown) return;

        LanternSystem lantern = player.GetComponent<LanternSystem>();
        if (lantern == null)
        {
            Debug.LogWarning("Player missing LanternSystem.");
            return;
        }

        bool success = lantern.Toggle();
        if (!success)
            Debug.Log("Lantern is empty.");

        lastToggleTime = Time.time;
    }
}
