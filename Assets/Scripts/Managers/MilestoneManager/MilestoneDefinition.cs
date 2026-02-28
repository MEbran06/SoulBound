using UnityEngine;

[CreateAssetMenu(fileName = "MilestoneDefinition", menuName = "Scriptable Objects/MilestoneDefinition")]
public class MilestoneDefinition : ScriptableObject
{
    [Min(1)] public int milestoneID;
}
