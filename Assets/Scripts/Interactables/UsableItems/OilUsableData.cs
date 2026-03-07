using UnityEngine;

[CreateAssetMenu(menuName = "Items/Usables/Oil Refill")]
public class OilUsableData : UsableItemData
{
    [SerializeField] private float fuelAdded = 25f;

    public override void Use(PlayerController player, Inventory inventory)
    {
        if (player == null || inventory == null) return;

        LanternSystem lantern = player.GetComponent<LanternSystem>();
        if (lantern == null)
        {
            Debug.LogWarning("Player missing LanternSystem.");
            return;
        }

        // Consume 1 from the currently equipped hotbar slot
        bool consume = inventory.TryConsumeEquipped(1);
        if (!consume) return;

        lantern.AddFuel(fuelAdded);
    }
}
