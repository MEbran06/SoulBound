using System.Collections.Generic;
using UnityEngine;
using Items.Ghosts;
using System;
using UnityEngine.LowLevel;
using Ghosts.Emotions;

public enum DifficultyChannel
{
    AggressionRate,
    AwarenessGain,
    ConfusionDecay,
    ChaseCooldown
}

// class for handling difficulty of the ghosts (modified by child attachment)
public class DifficultyProfile
{
    // each multiplier is driven by the child attachment (0-1), but its range is determined per personality
    private readonly Dictionary<DifficultyChannel, float> mult = new();

    public DifficultyProfile()
    {
        foreach (DifficultyChannel ch in Enum.GetValues(typeof(DifficultyChannel)))
            mult[ch] = 1f;
    }

    public float Get(DifficultyChannel ch) => mult[ch];
    public void Set(DifficultyChannel ch, float value) => mult[ch] = value;
}

public class GhostContext
{
    public GhostEmotion emotion;
    public DifficultyProfile difficulty;

    public bool canSeePlayer;
    //public bool canHearPlayer;
    public InsanitySystem insanitySystem;

    public Vector3 lastKnownPlayerPosition;
    public float lastTimePlayerSeen;
    //public float lastTimePlayerHeard;

    public float distanceToPlayer;
    public float awarenessLevel;

    public Vector3 currentTargetPosition;
    public bool playerIsHidden;
    // max distance between the ghost and the target point
    public float maxTargetDistance;

    public HideSpot playerHideSpot = null;

    // Search state runtime
    public bool searchComplete = false;
    public float searchEndTime = -Mathf.Infinity;


    public Vector3 lastHeardPosition;
    public float lastHeardTime;
    public float lastHeardLoudness;     // 0-1
    public bool heardNoiseThisFrame;
    public bool noiseTriggeredSearch;

    // Child ghost
    public bool childHasActiveRequest;      // specific request currently active
    public int childRequestedItemId;       // Id of the item requested by child
    public bool childInteractionAllowed;    // can the player interact if summoned (delivery window)
    public float childNextAllowedRequestTime;

    public GhostContext(InsanitySystem insanity)
    {
        emotion = new GhostEmotion();
        difficulty = new DifficultyProfile();
        // initial assumptions
        canSeePlayer = false;
        lastKnownPlayerPosition = Vector3.zero;
        lastTimePlayerSeen = -Mathf.Infinity; // never seen the player before
        distanceToPlayer = -1f; // undefined
        awarenessLevel = 0f;
        playerIsHidden = false;
        maxTargetDistance = 0.5f;
        insanitySystem = insanity;
        childHasActiveRequest = false;
        childInteractionAllowed = false;
        childNextAllowedRequestTime = 0f;
        childRequestedItemId = -1;

    }

    public bool CanEnterRequestWindow()
    {
        if (childInteractionAllowed) return false;
        if (Time.time < childNextAllowedRequestTime) return false;
        return true;
    }
}