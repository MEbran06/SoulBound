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

    [Header("Distance Tuning")]
    [Tooltip("Minimum distance of the ghost from the player for respawn")]
    [SerializeField] private float minDistanceFromPlayer = 12f;

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

    private static void TeleportGhost(GhostController ghost, Vector3 pos, Quaternion rot)
    {
        if (!ghost) return;

        var agent = ghost.agent; // your NavMeshAgent
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();

            // Disable agent before moving transform
            agent.enabled = false;
        }

        ghost.transform.SetPositionAndRotation(pos, rot);

        if (agent != null)
        {
            agent.enabled = true;

            // Warp seats the agent to navmesh at the new position
            agent.Warp(pos);

            agent.isStopped = false;
            agent.ResetPath();
        }

        // Clear "immediate chase" memory
        ghost.context.canSeePlayer = false;
        ghost.context.lastTimePlayerSeen = -999f;
        ghost.lastTimeHadLOS = -999f;

        // clear noise memory too
        ghost.context.lastHeardTime = -999f;

        //clear Search bookkeeping
        ghost.context.searchComplete = true;
    }

    public void RespawnGhostFarFromPlayer(GhostController ghost, Transform[] spawnPoints)
    {
        if (!ghost || spawnPoints == null || spawnPoints.Length == 0) return;

        Vector3 p = CheckpointManager.Instance.player.position;

        Transform best = null;
        float bestD2 = -1f;

        float minD2 = minDistanceFromPlayer * minDistanceFromPlayer;

        foreach (var sp in spawnPoints)
        {
            if (!sp) continue;
            float d2 = (sp.position - p).sqrMagnitude;

            // Prefer anything beyond min distance
            if (d2 >= minD2 && d2 > bestD2)
            {
                bestD2 = d2;
                best = sp;
            }
        }

        // Fallback: if none are far enough, just take the farthest
        if (best == null)
        {
            foreach (var sp in spawnPoints)
            {
                if (!sp) continue;
                float d2 = (sp.position - p).sqrMagnitude;
                if (d2 > bestD2)
                {
                    bestD2 = d2;
                    best = sp;
                }
            }
        }

        TeleportGhost(ghost, best.position, best.rotation);
    }
}