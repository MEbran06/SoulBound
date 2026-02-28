using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    public PlayerController player;
    public CameraController cameraController;

    public GameObject hotbarObj;
    public GameObject inventorySlotParent;
    public GameObject container;

    public Image dragIcon;

    private int equippedHotbarIndex = 0; // goes from 0-5
    public float equippedOpacity = 0.9f;
    public float normalOpacity = 0.58f;
    public Transform hand;
    private GameObject currentHandItem;

    private List<Slot> inventorySlots = new List<Slot>();
    private List<Slot> hotbarSlots = new List<Slot>();
    private List<Slot> allSlots = new List<Slot>();

    private float ghostLastUseTime = -Mathf.Infinity;
    
    private Slot draggedSlot = null;
    private bool isDragging = false;

    private ItemSO itemOnHand = null;

    public static Inventory Instance { get; private set; }
    public GameObject CurrentHandItem => currentHandItem;
    public ItemSO ItemOnHand => itemOnHand;

    private void Awake()
    {
        if (dragIcon != null) dragIcon.raycastTarget = false;

        Instance = this;

        inventorySlots.AddRange(inventorySlotParent.GetComponentsInChildren<Slot>());
        hotbarSlots.AddRange(hotbarObj.GetComponentsInChildren<Slot>());


        allSlots.AddRange(inventorySlots);
        allSlots.AddRange(hotbarSlots);    
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool opening = !container.activeSelf;
            container.SetActive(opening);

            Cursor.visible = opening;
            Cursor.lockState = opening ? CursorLockMode.None : CursorLockMode.Locked;

            if (player != null)
                player.InputDisabled = opening;
            if (cameraController != null)                
                cameraController.enabled = !opening;
        }

        bool uiOpen = container != null && container.activeSelf;
        if (uiOpen)
        {
            StartDrag();
            UpdateDragItemPosition();
            EndDrag();
        }
        else
        {
             if (dragIcon != null) dragIcon.enabled = false;
             isDragging = false;
             draggedSlot = null;
        }

        HandleHotBarSelection();
        HandleDropEquippedItem();
        UpdateHotbarOpacity();
    }

    public void AddItem(ItemSO itemToAdd, int amount)
    {
        if (itemToAdd == null || amount <= 0) return;

        int remaining = amount;

        // Hotbar first (stack then empty)
        remaining = AddIntoSlotList(hotbarSlots, itemToAdd, remaining);
        // Inventory grid next
         remaining = AddIntoSlotList(inventorySlots, itemToAdd, remaining);

        if (remaining > 0)
            Debug.Log("Not enough space in inventory for " + remaining + " of " + itemToAdd.itemName);

        RefreshAfterInventoryChange();
    }

    private int AddIntoSlotList(List<Slot> slots, ItemSO itemToAdd, int remaining)
    {
        // stack into existing
        foreach (Slot slot in slots)
        {
            if (remaining <= 0) break;
            if (!slot.HasItem() || slot.GetItem() != itemToAdd) continue;

            int max = itemToAdd.maxStackSize;
            int space = max - slot.GetAmount();
            if (space <= 0) continue;

            int add = Mathf.Min(space, remaining);
            slot.SetItem(itemToAdd, slot.GetAmount() + add);
            remaining -= add;
        }

        // fill empty
        foreach (Slot slot in slots)
        {
            if (remaining <= 0) break;
            if (slot.HasItem()) continue;

            int place = Mathf.Min(itemToAdd.maxStackSize, remaining);
            slot.SetItem(itemToAdd, place);
            remaining -= place;
        }

        return remaining;
}

    private void RefreshAfterInventoryChange()
    {
        UpdateHotbarOpacity();
        EquipHandItem();
    }

    private void StartDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Slot hovered = GetHoveredSlot();

            if (hovered != null && hovered.HasItem())
            {
                draggedSlot = hovered;
                isDragging = true;

                // Show drag item
                dragIcon.sprite = hovered.GetItem().icon;
                dragIcon.color = new Color(1, 1, 1, 0.5f);
                dragIcon.enabled = true;
            }
        }
    }

    private void EndDrag()
    {
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            Slot hovered = GetHoveredSlot();

            if (hovered !=  null)
            {
                HandleDrop(draggedSlot, hovered);

                dragIcon.enabled = false;

                draggedSlot = null;
                isDragging = false;
            }
        }
    }

    private Slot GetHoveredSlot()
    {
        foreach(Slot s in allSlots)
        {
            if (s.hovering)
                return s;
        }
        return null;
    }

    private void HandleDrop(Slot from, Slot to)
    {
        if (from == to) return;

        // Stacking
        if (to.HasItem() && to.GetItem() == from.GetItem())
        {
            int max = to.GetItem().maxStackSize;
            int space = max - to.GetAmount();

            if(space > 0)
            {
                int move = Mathf.Min(space, from.GetAmount());

                to.SetItem(to.GetItem(), to.GetAmount() + move);
                from.SetItem(from.GetItem(), from.GetAmount() - move);

                EquipHandItem();
                UpdateHotbarOpacity();

                if(from.GetAmount() <= 0)
                    from.ClearSlot();

                return;
            }
        }

        // Different Item
        if (to.HasItem())
        {
            ItemSO tempItem = to.GetItem();
            int tempAmount = to.GetAmount();

            to.SetItem(from.GetItem(), from.GetAmount());
            from.SetItem(tempItem, tempAmount);
            return;
        }

        // Empty Slot
        to.SetItem(from.GetItem(), from.GetAmount());
        from.ClearSlot();
    }

    private void UpdateDragItemPosition()
    {
        if (isDragging)
        {
            dragIcon.transform.position = Input.mousePosition;
        }
    }


    private void UpdateHotbarOpacity()
    {
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            Image icon = hotbarSlots[i].GetComponent<Image>();
            if (icon != null)
            {
                icon.color = (i == equippedHotbarIndex) ? new Color(1, 1, 1, equippedOpacity) : new Color(0.5f, 0.5f, 0.5f, normalOpacity);
            }
        }
    }

    private void HandleHotBarSelection()
    {
        for (int i = 0; i < 6; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                equippedHotbarIndex = i;
                UpdateHotbarOpacity();
                EquipHandItem();
            }
        }
    }

    private bool TryAddToHotbarFirst(ItemSO itemToAdd, int amount)
    {
    // stack into existing hotbar stacks first
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            Slot s = hotbarSlots[i];
            if (!s.HasItem()) continue;
            if (s.GetItem() != itemToAdd) continue;

            int max = itemToAdd.maxStackSize;
            int space = max - s.GetAmount();
            if (space <= 0) continue;

            int add = Mathf.Min(space, amount);
            s.SetItem(itemToAdd, s.GetAmount() + add);
            amount -= add;

            if (amount <= 0)
            {
                RefreshAfterInventoryChange();
                return true;
            }
        }

        // put into empty hotbar slots
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            Slot s = hotbarSlots[i];
            if (s.HasItem()) continue;

            int place = Mathf.Min(itemToAdd.maxStackSize, amount);
            s.SetItem(itemToAdd, place);
            amount -= place;

            if (amount <= 0)
            {
                RefreshAfterInventoryChange();
                return true;
            }
        }

        
        RefreshAfterInventoryChange();
        return false;
    }

    private void HandleDropEquippedItem()
    {
        if (!Input.GetKeyDown(KeyCode.Q)) return;
        if (container != null && container.activeSelf) return; 

        Slot equippedSlot = hotbarSlots[equippedHotbarIndex];
        if (!equippedSlot.HasItem()) return;

        ItemSO itemSO = equippedSlot.GetItem();
        if (itemSO == null || itemSO.itemPrefab == null) return;

        Vector3 spawnPos = cameraController.transform.position + cameraController.transform.forward * 1.2f;
        GameObject dropped = Instantiate(itemSO.itemPrefab, spawnPos, Quaternion.identity);

        DistanceScalerUI scaler = dropped.GetComponentInChildren<DistanceScalerUI>();
        if (scaler != null)
        {
            scaler.player = player.transform;
        }

        // Ensure Item component exists and is filled
        Item worldItem = dropped.GetComponent<Item>();
        if (worldItem == null) worldItem = dropped.AddComponent<Item>();
        worldItem.item = itemSO;
        worldItem.amount = equippedSlot.GetAmount();

        // set layer so PlayerInteraction can detect it (match interactLayer)
        // Put interactable layer name here:
        dropped.layer = LayerMask.NameToLayer("Interactable");

        equippedSlot.ClearSlot();
        RefreshAfterInventoryChange();
    }

    private void EquipHandItem()
    {
        if (currentHandItem != null) Destroy(currentHandItem);

        Slot equippedSlot = hotbarSlots[equippedHotbarIndex];
        if (!equippedSlot.HasItem()) return;

        ItemSO item = equippedSlot.GetItem();
        if (item.handItemPrefab == null) return;

        currentHandItem = Instantiate(item.handItemPrefab, hand);
        currentHandItem.transform.localPosition = item.handPositionOffset;
        currentHandItem.transform.localRotation = Quaternion.Euler(item.handRotationOffset);
        currentHandItem.transform.localScale = item.handScale;

        // store the item currently equiped
        itemOnHand = item;

        DisablePhysicsOnEquipped(currentHandItem);
    }

    public void UseEquippedItem()
    {
        Slot equippedSlot = hotbarSlots[equippedHotbarIndex];
        if (!equippedSlot.HasItem()) return;

        ItemSO so = equippedSlot.GetItem();
        if (so == null) return;

        Debug.Log($"Using hotbar item: {so.itemName}, isGhostItem={so.isGhostItem}, hasData={so.ghostItemData != null}");

        // Ghost use
        if (so.isGhostItem && so.ghostItemData != null)
        {
            float cd = Mathf.Max(0f, so.cooldownDuration);
            if (Time.time < ghostLastUseTime + cd) return;

            GhostItem.Activate(so.ghostItemData, player.transform.position);
            ghostLastUseTime = Time.time;
        }
    }

    private void DisablePhysicsOnEquipped(GameObject go)
    {
        if (go == null) return;

        foreach (var rb in go.GetComponentsInChildren<Rigidbody>(true))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        foreach (var col in go.GetComponentsInChildren<Collider>(true))
        {
            col.enabled = false;
        }
    }

    // consume the currently held item
    public void ConsumeCurrentItem()
    {
        if (!currentHandItem) return;

        Destroy(currentHandItem);
        currentHandItem = null;
    }
}
