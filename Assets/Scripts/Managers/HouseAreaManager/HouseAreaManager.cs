using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseAreaManager : MonoBehaviour
{
    public static HouseAreaManager Instance;
    public Dictionary<int, Area> areaList;
    private void Awake()
    {
        areaList = new Dictionary<int, Area>();
        Instance = this;
    }
}
