using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GhostManager : MonoBehaviour
{
    public static GhostManager Instance { get; private set; }

    [Header("Ghost References")]
    [Tooltip("All ghosts that should be affected by child attachment difficulty (e.g., Mom, Dad).")]
    public List<GhostController> ghosts = new List<GhostController>();

    [Header("Update Settings")]
    [Tooltip("How often to re-apply difficulty (Hz). 5-10 is plenty.")]
    [SerializeField] private float updateRateHz = 5f;

    [Tooltip("Smooth attachment input so difficulty doesn't snap. 0 = no smoothing.")]
    [SerializeField] private float attachmentSmoothing = 8f;

    private float nextUpdateTime;
    private float smoothedAttachment01 = 0.5f;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        nextUpdateTime = Time.time;
        smoothedAttachment01 = GetAttachment01Safe();
    }

    private void Update()
    {
        if (Time.time < nextUpdateTime) return;
        // update attachment less frequently 
        float dt = Mathf.Max(0.0001f, 1f / Mathf.Max(0.1f, updateRateHz));
        nextUpdateTime = Time.time + dt;

        float targetAttachment01 = GetAttachment01Safe();

        // Smooth input
        if (attachmentSmoothing > 0f)
        {
            float k = 1f - Mathf.Exp(-attachmentSmoothing * dt);
            smoothedAttachment01 = Mathf.Lerp(smoothedAttachment01, targetAttachment01, k);
        }
        else
        {
            smoothedAttachment01 = targetAttachment01;
        }
        ApplyDifficultyToGhosts(smoothedAttachment01);
    }

    private float GetAttachment01Safe()
    {
        if (GameManager.Instance == null) return 0.5f;
        return Mathf.Clamp01(GameManager.Instance.ChildAttachment01);
    }

    private void ApplyDifficultyToGhosts(float attachment01)
    {
        for (int i = 0; i < ghosts.Count; i++)
        {
            var ghost = ghosts[i];
            if (ghost == null || ghost.personality == null || ghost.context == null)
                continue;

            // Skips the child 
            if (ghost.CompareTag("ChildGhost")) continue;

            // Personality writes multipliers into ghost.context.difficulty
            ghost.personality.ApplyAttachmentDifficulty(ghost.context, attachment01);
        }
    }

    public void ResetGhostAfterCaught(GhostController ghost)
    {
        if (!ghost) return;
        ghost.ResetGhost(true);
    }

    public void ResetAllGhostsAfterCaught()
    {
        foreach (var ghost in ghosts)
        {
            ResetGhostAfterCaught(ghost);
        }
    }

    public GhostController getGhostByTag(string tag)
    {
        GameObject ghost = GameObject.FindGameObjectWithTag(tag);
        
        if (ghost == null) 
            return null;
        else 
            return ghost.GetComponent<GhostController>();
    }
}