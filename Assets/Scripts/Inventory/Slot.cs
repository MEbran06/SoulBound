using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // maintain unique Ids for each slot
    [SerializeField] private string slotIdString;
    public Guid UniqueId => Guid.Parse(slotIdString);

    public bool hovering;

    private ItemSO heldItem;
    private int itemAmount;

    private Image iconImage;
    private TextMeshProUGUI amountTxt;


#if UNITY_EDITOR
    private void OnValidate()
    {
        // Generate once, then never touch again unless you explicitly reset
        if (string.IsNullOrEmpty(slotIdString))
            slotIdString = Guid.NewGuid().ToString();
    }

    [ContextMenu("Reset Slot GUID (DANGEROUS)")]
    private void ResetGuid()
    {
        slotIdString = Guid.NewGuid().ToString();
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif


    private void Awake()
    {
        iconImage = transform.GetChild(0).GetComponent<Image>();
        amountTxt = transform.GetChild(1).GetComponent<TextMeshProUGUI>();

        // register all slots so we can restore Inventory
        SlotRegistry.Register(this);
    }

    private void OnDestroy()
    {
        SlotRegistry.Unregister(this);
    }

    public ItemSO GetItem()
    {
        return heldItem;
    }

    public int GetAmount()
    {
        return itemAmount;
    }

    public void SetItem(ItemSO item, int amount = 1)
    {
        heldItem = item;
        itemAmount = amount;

        UpdateSlot();
    }

    public void UpdateSlot()
    {
        if (iconImage == null)
        {
            iconImage = transform.GetChild(0).GetComponent<Image>();
            amountTxt = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        }

        if (heldItem != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = heldItem.icon;
            amountTxt.text = itemAmount.ToString();
        }
        else
        {
            iconImage.enabled = false;
            amountTxt.text = "";
        }
    }

    public int AddAmount(int amountToAdd)
    {
        itemAmount += amountToAdd;
        UpdateSlot();
        return itemAmount;
    }

    public int RemoveAmount(int amountToRemove)
    {
        itemAmount -= amountToRemove;
        if (itemAmount <= 0)
        {
            ClearSlot();
        }
        else
        {
            UpdateSlot();
        }

            return itemAmount;
    }

    public void ClearSlot()
    {
        heldItem = null;
        itemAmount = 0;
        UpdateSlot();
    }

    public bool HasItem()
    {
        return heldItem != null;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }
}
