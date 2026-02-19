using UnityEngine;

namespace Items.Ghosts
{
    public enum EmotionType
    {
        Aggression,
        Confusion
    }

    [System.Serializable]
    public struct EmotionModifier
    {
        public EmotionType emotion;
        public float value;
    }

    [CreateAssetMenu(menuName = "Ghosts/GhostItemData")]
    public class GhostItemData : ScriptableObject
    {
        public EmotionModifier[] modifiers;
        public float activationRadius;
    }


}