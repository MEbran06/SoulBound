using System;
using System.Linq;
using UnityEngine;
using Ghosts.Emotions;

namespace Items.Ghosts
{
    public enum ChildItemCategory
    {
        Neutral,
        ChildMemory,
        FamilyMemory,
        ParentItem,
        OccultItem
    }

    [System.Serializable]
    public struct EmotionModifier
    {
        public EmotionType emotion;
        [Range(-100f, 100f)] public float value;
    }

    [System.Serializable]
    public struct ChildCategoryEffect
    {
        public ChildItemCategory category;
        public float attachmentDelta;
    }

    [CreateAssetMenu(menuName = "Ghosts/GhostItemData")]
    public class GhostItemData : ScriptableObject
    {
        public EmotionModifier[] modifiers;
        public float activationRadius;

        [Header("Child Item Data")]
        // Use these fields instead of modifiers ONLY on child ghost items
        public bool canBeGivenToChild;
        [Min(1)] public int childItemId;
        public ChildItemCategory childCategory = ChildItemCategory.Neutral;

        [Tooltip("List of modifiers on attachement level of the child when the item is applied")]
        public ChildCategoryEffect[] childCategoryModifiers;
        [Tooltip("The earliest story step this item makes sense")]
        public int minMilestoneIndex = 0;

        private void OnValidate()
        {
            int max = Enum.GetValues(typeof(ChildItemCategory)).Length;
            if (childCategoryModifiers != null && childCategoryModifiers.Length > max)
            {
                // truncate the list if it exceeds the limit
                childCategoryModifiers = childCategoryModifiers.Take<ChildCategoryEffect>(max).ToArray();
            }
        }
    }


}