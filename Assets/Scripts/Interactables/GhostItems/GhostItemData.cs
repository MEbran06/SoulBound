using UnityEngine;

namespace Items.Ghosts
{
    public enum EmotionType
    {
        Aggression,
        Confusion
    }
    [System.Serializable]
    public struct EmotionValues
    {
        public EmotionType emotion;
        [Range(0f, 100f)] public float value;
    }

    [System.Serializable]
    public struct EmotionModifier
    {
        public EmotionType emotion;
        [Range(-100f, 100f)] public float value;
    }

    [CreateAssetMenu(menuName = "Ghosts/GhostItemData")]
    public class GhostItemData : ScriptableObject
    {
        public EmotionModifier[] modifiers;
        public float activationRadius;
    }


}