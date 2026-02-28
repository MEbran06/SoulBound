using Items.Ghosts;
using UnityEngine;
using System.Collections;
using Ghosts.Emotions;

public class ChildInteraction : Interactable
{
    [SerializeField] GhostController child;
    [SerializeField] const float SPECIFIC_REQUEST_BONUS = 15f;
    [SerializeField] float noItemGivenPunishment = 15f;

    private void Start()
    {
        promptMessage = "Press E to Give Item";
    }

    public override void Interact(PlayerController player)
    {
        // player gives currently held/selected item
        TryReceiveGift(player);
    }

    public void TryReceiveGift(PlayerController player)
    {
        if (child.context == null) return;

        if (!child.context.childInteractionAllowed)
            return;

        Item heldItem = player.CurrentHeldItem;
        if (heldItem == null)
            return;

        GhostItem ghostItem = heldItem as GhostItem;
        if (ghostItem == null)
        {
            child.context.emotion.AddFromItem(EmotionType.Attachment, -noItemGivenPunishment);
            return;
        }

        EvaluateGift(ghostItem.Data);

        player.RemoveItem();  // consume item
    }

    public void EvaluateGift(GhostItemData data)
    {
        float delta = 0f;
        foreach (var effect in data.childCategoryModifiers)
        {
            if (effect.category == data.childCategory)
            {
                delta = effect.attachmentDelta;
                break;
            }
        }

        // Specific request bonus
        if (child.context.childHasActiveRequest &&
            data.childItemId == child.context.childRequestedItemId)
        {
            delta += SPECIFIC_REQUEST_BONUS;
            child.context.childHasActiveRequest = false;
        }

        child.context.emotion.AddFromItem(EmotionType.Attachment, delta);
    }
}
