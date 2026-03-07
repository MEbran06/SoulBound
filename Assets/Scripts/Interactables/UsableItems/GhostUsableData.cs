using UnityEngine;
using Items.Ghosts;

[CreateAssetMenu(menuName = "Items/Usables/Ghost")]
public class GhostUsableData : UsableItemData
{
    public GhostItemData ghostItemData;
    public float cooldownDuration = 5f;

    public override void Use(PlayerController player, Inventory inventory)
    {
        if (ghostItemData == null || player == null) return;

        ItemSO item = inventory.ItemOnHand;
        if (item == null) return;

        if (!inventory.CanUseItem(item, cooldownDuration))
            return;

        GhostItem.Activate(ghostItemData, player.transform.position);
        inventory.MarkItemUsed(item);
    }
}
