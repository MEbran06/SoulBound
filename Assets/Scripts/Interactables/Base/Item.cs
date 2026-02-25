using UnityEngine;

public class Item : Interactable
{
    protected bool isHeld = false;
    protected PlayerController owner;
    public ItemSO item;
    public int amount = 1;

    public override void Interact(PlayerController player)
    {
        player.AddToInventory(this);
        player.HoldItem(this);

        // Disable physics while held
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        owner = player;
    }

    public virtual void OnHeld()
    {
        isHeld = true;
    }

    public virtual void OnDropped()
    {
        isHeld = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = false;

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = true;
    }

    public virtual void Use()
    {
        // Default: do nothing (collectible items)
    }
}

