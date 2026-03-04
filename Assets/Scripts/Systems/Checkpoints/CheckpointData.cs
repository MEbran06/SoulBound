using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SavedPickedupItemsState
{
    public string itemName;
    public string SlotID;
    public bool isCollected;
    public int amount;
}


[Serializable]
public class CheckpointData
{
    public int milestoneId;         // last milestone entered
    public Vector3 position;
    public Quaternion rotation;
    public Dictionary<Guid, SavedPickedupItemsState> currentItems;
    public List<string> collectedWorldItemIds = new();
}