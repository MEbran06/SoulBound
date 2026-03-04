using UnityEngine;
using Items.Ghosts;

[CreateAssetMenu(menuName = "Items/Usables/Ghost")]
public class GhostUsableData : UsableItemData
{
    public GhostItemData ghostItemData;
    public float cooldownDuration = 5f;

    private float lastUseTime = -Mathf.Infinity;

    public override void Use(PlayerController player, Inventory inventory)
    {
        if (ghostItemData == null || player == null) return;

        if (Time.time < lastUseTime + cooldownDuration) return;

        GhostItem.Activate(ghostItemData, player.transform.position);
        lastUseTime = Time.time;
    }
}
