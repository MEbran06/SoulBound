using System;
using UnityEngine;

[Serializable]
public class CheckpointData
{
    public int milestoneId;         // last milestone entered
    public Vector3 position;
    public Quaternion rotation;
}