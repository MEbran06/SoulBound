using UnityEngine;

public abstract class UsableItemData : ScriptableObject
{
    public abstract void Use(PlayerController player, Inventory inventory);
}
