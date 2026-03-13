using System.IO;
using UnityEngine;
using System;

public class MetricsLogger : MonoBehaviour
{
    private float startTime;
    private int dadGhostEncounters = 0;
    private int momGhostEncounters = 0;
    private int momHallucinations = 0;

    public static MetricsLogger Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        startTime = Time.time;

        if (GameManager.Instance != null)
            GameManager.Instance.OnSessionEnded += HandleSessionEnded;
        else
            Debug.LogError("MetricsLogger: GameManager.Instance is null in Start.");
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnSessionEnded -= HandleSessionEnded;
    }

    public void RegisterDadGhostEncounter()
    {
        dadGhostEncounters++;
    }

    public void RegisterMomHallucination()
    {
        momHallucinations++;
    }

    public void RegisterMomGhostEncounter()
    {
        momGhostEncounters++;
    }

    private void HandleSessionEnded(string result)
    {
        Debug.Log("MetricsLogger received OnSessionEnded: " + result);
        SaveSession(result);
    }

    public void SaveSession(string result)
    {
        float playtime = Time.time - startTime;

        string buildFolder = Directory.GetParent(Application.dataPath).FullName;
        string path = Path.Combine(buildFolder, "metrics.txt");

        if (!File.Exists(path))
        {
            File.WriteAllText(path, "Timestamp,Result,Playtime (sec),DadGhostEncounters,MomGhostEncounters,MomHallucinations\n");
        }

        string row =
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "," +
            result + "," +
            playtime + "," +
            dadGhostEncounters + "," +
            momGhostEncounters + "," +
            momHallucinations + "\n";

        File.AppendAllText(path, row);

        Debug.Log("Metrics saved to: " + path);

        ResetSession();
    }

    private void ResetSession()
    {
        startTime = Time.time;
        dadGhostEncounters = 0;
        momGhostEncounters = 0;
        momHallucinations = 0;
    }
}