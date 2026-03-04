using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject root;            // whole pause menu panel
    [SerializeField] private TMP_Dropdown checkpointDropdown;

    [Header("Input")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    private bool isOpen;
    private List<int> milestoneIds = new();

    void Start()
    {
        if (root != null) root.SetActive(false);
        isOpen = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (isOpen) Close();
            else Open();
        }
    }

    public void Open()
    {
        isOpen = true;
        if (root != null) root.SetActive(true);

        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // optional: disable player input
        if (CheckpointManager.Instance != null && CheckpointManager.Instance.player != null)
        {
            var pc = CheckpointManager.Instance.player.GetComponentInChildren<PlayerController>();
            if (pc != null) pc.InputDisabled = true;
        }
    }

    public void Close()
    {
        isOpen = false;
        if (root != null) root.SetActive(false);

        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // re-enable player input
        if (CheckpointManager.Instance != null && CheckpointManager.Instance.player != null)
        {
            var pc = CheckpointManager.Instance.player.GetComponentInChildren<PlayerController>();
            if (pc != null) pc.InputDisabled = false;
        }
    }

    // Button: "Load checkpoints"
    public void RefreshCheckpointList()
    {
        milestoneIds = SaveSystem.ListSavedMilestones();

        checkpointDropdown.ClearOptions();

        var labels = new List<string>();
        foreach (var id in milestoneIds)
            labels.Add(id < 0 ? "Start" : $"Milestone {id}");

        if (labels.Count == 0)
        {
            labels.Add("Start");
            milestoneIds = new List<int> { -1 };
        }

        checkpointDropdown.AddOptions(labels);
        checkpointDropdown.value = labels.Count - 1; // default latest
        checkpointDropdown.RefreshShownValue();
    }

    // Button: "Load selected"
    public void LoadSelectedCheckpoint()
    {
        if (milestoneIds == null || milestoneIds.Count == 0)
            RefreshCheckpointList();

        int idx = checkpointDropdown.value;
        idx = Mathf.Clamp(idx, 0, milestoneIds.Count - 1);

        int chosen = milestoneIds[idx];

        CheckpointManager.Instance.LoadCheckpointByMilestone(chosen);

        Close();
    }

    // Optional: button "Start new"
    public void StartNewRun()
    {
        CheckpointManager.Instance.LoadCheckpointByMilestone(-1);
        SaveSystem.ClearAll();
        CheckpointManager.Instance.activeMilestoneIdForRespawn = -1;
        PlayerPrefs.SetInt(CheckpointManager.Instance.ActiveKey, -1);
        PlayerPrefs.Save();
        Close();
    }

    // button "Quit"
    public void QuitGame()
    {
        Time.timeScale = 1f; // make sure time is restored
        // go back to my main menu
        SceneManager.LoadScene("Menu");
    }
}