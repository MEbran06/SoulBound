using UnityEngine;

public class Item : Interactable
{
    public ItemSO item;
    public int amount = 1;

    private void Start()
    {
        promptMessage = "Press E to Pick Up";
    }

    public override void Interact(PlayerController player)
    {
        if (item == null) 
        {
            Debug.LogError("ItemSO not assigned on " + gameObject.name);
            return;
        }
        if (Inventory.Instance == null)
        {
            Debug.LogError("Inventory.Instance is null.");
            return;
        }
        Inventory.Instance.AddItem(item, amount);
        Destroy(gameObject);
    }

    // public virtual void OnHeld()
    // {
    //     isHeld = true;
    // }

    // public virtual void OnDropped()
    // {
    //     isHeld = false;

    //     Rigidbody rb = GetComponent<Rigidbody>();
    //     if (rb != null)
    //         rb.isKinematic = false;

    //     Collider col = GetComponent<Collider>();
    //     if (col != null)
    //         col.enabled = true;
    // }

    // public virtual void Use()
    // {
    //     // Default: do nothing (collectible items)
    // }
}

