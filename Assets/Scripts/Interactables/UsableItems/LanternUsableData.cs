using UnityEngine;

[CreateAssetMenu(menuName = "Items/Usables/Lantern Toggle")]
public class LanternUsableData : UsableItemData
{
    [SerializeField] private float toggleCooldown = 0.15f;

    public override void Use(PlayerController player, Inventory inventory)
    {
        if (player == null) return;

        ItemSO item = inventory.ItemOnHand;
        if (item == null) return;

        if (!inventory.CanUseItem(item, toggleCooldown))
            return;

        LanternSystem lantern = player.GetComponent<LanternSystem>();
        if (lantern == null)
        {
            Debug.LogWarning("Player missing LanternSystem.");
            return;
        }

        bool success = lantern.Toggle();
        if (success)
            inventory.MarkItemUsed(item);
    }
}
