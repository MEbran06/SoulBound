using UnityEngine;
using Items.Ghosts;

[CreateAssetMenu(fileName = "New Item", menuName = "NewItem")]

public class ItemSO : ScriptableObject
{
   public string itemName;
   public Sprite icon;
   public int maxStackSize;

   public GameObject itemPrefab;
   public GameObject handItemPrefab;

   public Vector3 handPositionOffset;
   public Vector3 handRotationOffset;
   public Vector3 handScale = Vector3.one;

   // public bool isGhostItem;
   // public GhostItemData ghostItemData;
   // public float cooldownDuration = 5f;

   public UsableItemData usableData;
}
