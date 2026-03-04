using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance { get; private set; }

    [Header("Assign ALL ItemSO assets here")]
    [SerializeField] private List<ItemSO> items = new();

    private Dictionary<string, ItemSO> byName;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Build();
    }

    private void Build()
    {
        byName = new Dictionary<string, ItemSO>();

        foreach (var it in items)
        {
            if (it == null) continue;

            // itemName must be unique
            if (byName.ContainsKey(it.itemName))
            {
                Debug.LogError($"[ItemDatabase] Duplicate itemName '{it.itemName}'. Fix this.", it);
                continue;
            }

            byName.Add(it.itemName, it);
        }

        Debug.Log($"[ItemDatabase] Loaded {byName.Count} items.");
    }

    public ItemSO GetByName(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return null;
        byName.TryGetValue(itemName, out var it);
        return it;
    }
}