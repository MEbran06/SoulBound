using UnityEngine;

[CreateAssetMenu(menuName = "Items/Usables/Lantern Toggle")]
public class LanternUsableData : UsableItemData
{
    public override void Use(PlayerController player, Inventory inventory)
    {
        if (player == null) return;

        LanternSystem lantern = player.GetComponent<LanternSystem>();
        if (lantern == null)
        {
            Debug.LogWarning("Player missing LanternSystem.");
            return;
        }

        bool success = lantern.Toggle();
        if (!success)
            Debug.Log("Lantern is empty.");
    }
}
