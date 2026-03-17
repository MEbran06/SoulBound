using Items.Ghosts;
using UnityEngine;
using System.Collections;
using Ghosts.Emotions;

public class ChildInteraction : Interactable
{
    [SerializeField] GhostController child;

    const float SPECIFIC_REQUEST_BONUS = 15f;
    const float MAX_INTERACTION_DURATION = 5f;
    [SerializeField] float noItemGivenPunishment = 15f;

    private Coroutine requestTimeoutRoutine;

    private void Start()
    {
        promptMessage = "Press E to Give Item";
    }

    public override void Interact(PlayerController player)
    {
        TryReceiveGift(player);
    }

    public void BeginRequestWindow()
    {
        if (child == null || child.context == null) return;

        child.context.childHasActiveRequest = true;

        if (requestTimeoutRoutine != null)
            StopCoroutine(requestTimeoutRoutine);

        requestTimeoutRoutine = StartCoroutine(RequestTimeoutCoroutine());
    }

    private IEnumerator RequestTimeoutCoroutine()
    {
        yield return new WaitForSeconds(MAX_INTERACTION_DURATION);

        if (child != null &&
            child.context != null &&
            child.context.childHasActiveRequest)
        {
            ApplyNoItemPunishment();
            child.context.childHasActiveRequest = false;
        }

        requestTimeoutRoutine = null;
    }

    public void TryReceiveGift(PlayerController player)
    {
        if (child == null || child.context == null) return;
        if (!child.context.childInteractionAllowed) return;

        ItemSO heldItem = player.inventoryUI.ItemOnHand;

        // Punish only if there is an active request and player tries with nothing
        if (heldItem == null)
        {
            if (child.context.childHasActiveRequest)
            {
                ApplyNoItemPunishment();
                CancelRequestWindow();
            }
            return;
        }

        // if (!heldItem.isGhostItem || heldItem.ghostItemData == null)
        //     return;

        // Only allow items intended for child interaction
        // if (!heldItem.ghostItemData.canBeGivenToChild)
        //     return;

        // EvaluateGift(heldItem.ghostItemData);
        // player.inventoryUI.ConsumeCurrentItem();
        GhostUsableData ghostUse = heldItem.usableData as GhostUsableData;
        if (ghostUse == null || ghostUse.ghostItemData == null)
        {
            child.context.emotion.AddFromItem(EmotionType.Attachment, -noItemGivenPunishment);
            return;
        }

        EvaluateGift(ghostUse.ghostItemData);

        player.inventoryUI.TryConsumeEquipped(1);
        // // Punish player if the item is not a ghost item
        // if (!heldItem.isGhostItem)
        // {
        //     child.context.emotion.AddFromItem(EmotionType.Attachment, -noItemGivenPunishment);
        // }
        // else
        // {
        //     EvaluateGift(heldItem.ghostItemData);
        //     player.inventoryUI.ConsumeCurrentItem();  // consume item
        // }
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

        // Extra bonus only if exact requested item was given
        if (child.context.childHasActiveRequest &&
            data.childItemId == child.context.childRequestedItemId)
        {
            delta += SPECIFIC_REQUEST_BONUS;
        }

        child.context.emotion.AddFromItem(EmotionType.Attachment, delta);

        // End request after any valid child item is given
        if (child.context.childHasActiveRequest)
        {
            CancelRequestWindow();
        }
    }

    private void ApplyNoItemPunishment()
    {
        child.context.emotion.AddFromItem(EmotionType.Attachment, -noItemGivenPunishment);
        //Debug.Log("Child request ignored. Applying attachment punishment.");
    }

    private void CancelRequestWindow()
    {
        child.context.childHasActiveRequest = false;

        if (requestTimeoutRoutine != null)
        {
            StopCoroutine(requestTimeoutRoutine);
            requestTimeoutRoutine = null;
        }
    }
}