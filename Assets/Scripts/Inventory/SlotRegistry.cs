using System;
using System.Collections.Generic;
using UnityEngine;

public static class SlotRegistry
{
    // register all slots with items by Id
    public static readonly Dictionary<Guid, Slot> ById = new();

    public static void Register(Slot p)
    {
        if (p == null ) return;

        // avoid re-registering the same slot
        if (ById.ContainsKey(p.UniqueId))
        {
            return;
        }

        ById.TryAdd(p.UniqueId, p);
    }

    public static void Unregister(Slot p)
    {
        if (p == null) return;
        if (ById.TryGetValue(p.UniqueId, out var cur) && cur == p)
            ById.Remove(p.UniqueId);
    }

    // find slot by item name
    public static Slot FindSlotByItemName(string itemName)
    {
        foreach (var slot in ById.Values)
        {
            if (slot.GetItem().itemName == itemName)
                return slot;
        }
        return null;
    }
}