using System.IO;
using UnityEngine;

public static class SaveSystem
{
    // path to store the data
    private static string Path => System.IO.Path.Combine(Application.persistentDataPath, "checkpoint.json");

    public static void SaveCheckpoint(CheckpointData data)
    {
        var json = JsonUtility.ToJson(data, true);
        File.WriteAllText(Path, json);
    }

    public static bool TryLoadCheckpoint(out CheckpointData data)
    {
        if (!File.Exists(Path))
        {
            Debug.LogWarning("Path doesn't exist");
            data = null;
            return false;
        }
        Debug.Log($"{Application.persistentDataPath}");
        var json = File.ReadAllText(Path);
        data = JsonUtility.FromJson<CheckpointData>(json);
        return data != null;
    }

    public static void Clear()
    {
        if (File.Exists(Path)) File.Delete(Path);
    }
}
