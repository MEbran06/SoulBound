using UnityEngine;

public class Area : MonoBehaviour
{
    public HouseArea area;

    void Start()
    {
        HouseAreaManager.Instance.areaList.Add(area.houseAreaId, this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FatherGhost"))
        {
            var ghost = other.GetComponentInParent<GhostController>();
            if (ghost != null)
                ghost.currentArea = area.houseAreaId;
        }

        if (other.CompareTag("Player"))
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player != null)
                player.currentHouseAreaId = area.houseAreaId;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("FatherGhost"))
        {
            var ghost = other.GetComponentInParent<GhostController>();
            if (ghost != null)
                ghost.currentArea = area.houseAreaId;
        }

        if (other.CompareTag("Player"))
        {
            var player = other.GetComponentInParent<PlayerController>();
            if (player != null)
                player.currentHouseAreaId = area.houseAreaId;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var player = other.GetComponentInParent<PlayerController>();
        if (player != null)
            player.currentHouseAreaId = -1;
    }
}
