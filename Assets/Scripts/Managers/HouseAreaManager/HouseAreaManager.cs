using System.Collections.Generic;
using UnityEngine;

public class HouseAreaManager : MonoBehaviour
{
    public static HouseAreaManager Instance;
    public List<Area> areaList;
    private void Awake()
    {
        Instance = this;
    }
}
