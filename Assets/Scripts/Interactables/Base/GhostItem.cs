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
        // prompt message for UI
        promptMessage = "Press E to Pick Up";
    }

    public override void Use()
    {
        if (!CanActivate())
        {
            return;
        }

        OnMemoryActivated?.Invoke(ghostItemData, owner.transform.position);
        lastActivationTime = Time.time;
    }

    private bool CanActivate()
    {
        return Time.time >= lastActivationTime + cooldownDuration;
    }

}
