using UnityEngine;

public class Area : MonoBehaviour
{
    public HouseArea area;

    void Start()
    {
        HouseAreaManager.Instance.areaList.Add(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().currentHouseAreaId = area.houseAreaId;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().currentHouseAreaId = -1;
        }
    }
}
