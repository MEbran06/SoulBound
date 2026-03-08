using UnityEngine;

[CreateAssetMenu(fileName = "HouseArea", menuName = "House/HouseArea")]
public class HouseArea : ScriptableObject
{
    [Min(1)] public int houseAreaId;
}
