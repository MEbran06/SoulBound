using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "NewItem")]

public class ItemSO : ScriptableObject
{
   public string itemName;
   public Sprite icon;
   public int maxStackSize;
   public GameObject itemPrefab;
   public GameObject handItemPrefab;
}
