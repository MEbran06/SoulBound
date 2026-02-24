using System.Collections.Generic;
using UnityEngine;
using Items.Ghosts;
using System;
using UnityEngine.LowLevel;
public class GhostContext
{
    public bool canSeePlayer;
    //public bool canHearPlayer;
    public InsanitySystem insanitySystem;

    public Vector3 lastKnownPlayerPosition;
    public float lastTimePlayerSeen;
    //public float lastTimePlayerHeard;

    public float distanceToPlayer;
    public float awarenessLevel;

    Dictionary<EmotionType, float> emotionStates;


    public Vector3 currentTargetPosition;
    public bool playerIsHidden;
    // max distance between the ghost and the target point
    public float maxTargetDistance;

    // passively reduce confusion over time (only increase with item interaction)
    public float confusionDecayRate;

    public HideSpot playerHideSpot = null;


    /*
    public bool playerMadeNoise;
    public bool ghostIsSearchingClosets;
    */

    public GhostContext(InsanitySystem insanity)
    {
        // initial assumptions
        canSeePlayer = false;
        lastKnownPlayerPosition = Vector3.zero;
        lastTimePlayerSeen = -Mathf.Infinity; // never seen the player before
        distanceToPlayer = -1f; // undefined
        awarenessLevel = 0f;
        emotionStates = getEmotionStates(); // create an empty dictionary to keep track of emotions
        playerIsHidden = false;
        confusionDecayRate = -0.5f;
        maxTargetDistance = 0.5f;
        insanitySystem = insanity;

    }
    public Dictionary<EmotionType, float> getEmotionStates()
    {
        Dictionary<EmotionType, float> emotions = new Dictionary<EmotionType, float>();

        // initialize this dictionary with 0s for all emotions
        foreach (EmotionType emotion in Enum.GetValues(typeof(EmotionType)))
        {
            emotions.Add(emotion, 0f);
        }

        return emotions;
    }

    public void ModifyEmotion(EmotionType emotion, float value)
    {
        // safete measure
        if (!emotionStates.ContainsKey(emotion))
            emotionStates[emotion] = 0f;

        emotionStates[emotion] += value;
        emotionStates[emotion] = Mathf.Clamp(emotionStates[emotion], 0f, 100f);
    }

    public float GetEmotion(EmotionType emotion)
    {
        return emotionStates[emotion];
    }

    public void SetEmotion(EmotionType emotion, float value)
    {
        emotionStates[emotion] = value;
    }

}