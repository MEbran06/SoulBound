using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public struct CheckpointItemEntry
{
    public string slotGuid;
    public SavedPickedupItemsState state;
}

[Serializable]
public class CheckpointDataFile
{
    public int milestoneId;
    public Vector3 position;
    public Quaternion rotation;

    public List<CheckpointItemEntry> currentItems = new();
    public List<string> collectedWorldItemIds = new();
}

public static class SaveSystem
{

    public static void SaveCheckpoint(CheckpointData data)
    {
        if (data == null)
        {
            Debug.LogError("[SaveSystem] SaveCheckpoint called with null data.");
            return;
        }

        var file = ToFile(data);
        var json = JsonUtility.ToJson(file, true);

        string path = PathForMilestone(data.milestoneId);
        Directory.CreateDirectory(Application.persistentDataPath);
        File.WriteAllText(path, json);

        Debug.Log($"[SaveSystem] Saved checkpoint to: {path} (items={file.currentItems?.Count ?? 0})");
        Debug.Log($"[SaveSystem] Company={Application.companyName} Product={Application.productName} PersistentPath={Application.persistentDataPath}");
    }

    // Loads "continue": latest milestone if present, else start, else none
    public static bool TryLoadCheckpoint(out CheckpointData data)
    {
        string path = GetBestExistingCheckpointPath();

        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            Debug.LogWarning($"[SaveSystem] No checkpoint file found. PersistentPath={Application.persistentDataPath}");
            data = null;
            return false;
        }

        var json = File.ReadAllText(path);
        var file = JsonUtility.FromJson<CheckpointDataFile>(json);

        if (file == null)
        {
            Debug.LogWarning($"[SaveSystem] Failed to parse checkpoint JSON at: {path}");
            data = null;
            return false;
        }

        data = FromFile(file);
        Debug.Log($"[SaveSystem] Loaded checkpoint from: {path} (items={data.currentItems?.Count ?? 0})");
        return true;
    }

    public static void Clear()
    {
        // get the last's checkpoint path
        string path = GetBestExistingCheckpointPath();
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
            File.Delete(path);
    }

    public static bool TryLoadCheckpointForMilestone(int milestoneId, out CheckpointData data)
    {
        string path = PathForMilestone(milestoneId);

        if (!File.Exists(path))
        {
            data = null;
            return false;
        }

        var json = File.ReadAllText(path);
        var file = JsonUtility.FromJson<CheckpointDataFile>(json);
        data = file != null ? FromFile(file) : null;
        return data != null;
    }

    public static void ClearAll()
    {
        string dir = Application.persistentDataPath;

        // milestones
        foreach (var f in Directory.GetFiles(dir, "checkpoint_milestone_*.json"))
            File.Delete(f);
    }

    public static List<int> ListSavedMilestones()
    {
        var list = new List<int>();
        var dir = Application.persistentDataPath;

        foreach (var file in Directory.GetFiles(dir, "checkpoint_milestone_*.json"))
        {
            var fn = System.IO.Path.GetFileNameWithoutExtension(file); // checkpoint_milestone_3
            var parts = fn.Split('_');
            if (int.TryParse(parts[^1], out int id))
                list.Add(id);
        }

        if (File.Exists(PathForMilestone(-1))) list.Add(-1); // start
        list.Sort();
        return list;
    }

    private static string PathForMilestone(int milestoneId)
    {
        string name = milestoneId < 0 ? "checkpoint_start.json" : $"checkpoint_milestone_{milestoneId}.json";
        return System.IO.Path.Combine(Application.persistentDataPath, name);
    }

    private static string GetBestExistingCheckpointPath()
    {
        // Prefer highest milestone file
        var ids = ListSavedMilestones();
        if (ids.Count == 0) return null;

        // if list contains -1 and others, the max will be the highest milestone (good)
        int best = ids[ids.Count - 1];
        return PathForMilestone(best);
    }

    private static CheckpointDataFile ToFile(CheckpointData src)
    {
        var dst = new CheckpointDataFile
        {
            milestoneId = src.milestoneId,
            position = src.position,
            rotation = src.rotation,
            currentItems = new List<CheckpointItemEntry>(),
            collectedWorldItemIds = new List<string>()
        };

        if (src.currentItems != null)
        {
            foreach (var kv in src.currentItems)
            {
                dst.currentItems.Add(new CheckpointItemEntry
                {
                    slotGuid = kv.Key.ToString(),
                    state = kv.Value
                });
            }
        }

        if (src.collectedWorldItemIds != null)
            dst.collectedWorldItemIds.AddRange(src.collectedWorldItemIds);

        return dst;
    }

    private static CheckpointData FromFile(CheckpointDataFile src)
    {
        var dst = new CheckpointData
        {
            milestoneId = src.milestoneId,
            position = src.position,
            rotation = src.rotation,
            currentItems = new Dictionary<Guid, SavedPickedupItemsState>(),
            collectedWorldItemIds = new List<string>()
        };

        if (src.currentItems != null)
        {
            foreach (var e in src.currentItems)
            {
                if (Guid.TryParse(e.slotGuid, out var id))
                    dst.currentItems[id] = e.state;
            }
        }

        if (src.collectedWorldItemIds != null)
            dst.collectedWorldItemIds.AddRange(src.collectedWorldItemIds);

        return dst;
    }
}