using Items.Ghosts;
using System;
using System.Collections;
using UnityEngine;

public class GhostItem : Item
{
    public static event Action<GhostItemData, Vector3> OnMemoryActivated;
    private float lastActivationTime = -Mathf.Infinity;
    [SerializeField] private GhostItemData ghostItemData;
    [SerializeField] private float cooldownDuration = 5f;
    public GhostItemData Data => ghostItemData;
    public void Start()
    {
        OnMemoryActivated?.Invoke(data, position);
    }
}
