using Items.Ghosts;
using System;
using System.Collections;
using UnityEngine;

public class GhostItem : Item
{
    public static event System.Action<GhostItemData, Vector3> OnMemoryActivated;

    public static void Activate(GhostItemData data, Vector3 position)
    {
        OnMemoryActivated?.Invoke(data, position);
    }
}
