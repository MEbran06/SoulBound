using AI.Ghosts.States;
using UnityEngine;

public class InsanitySystem : MonoBehaviour
{
    [Header("Insanity Settings")]
    public float maxInsanity = 100f;
    public float drainRate = 1f;
    public float recoveryRate = 3f;
    public float disturbanceThreshold = 80f;
    public float disturbanceRecoveryRate = 0.5f;
    public float disturbanceDrainRate = 5f;

    public float CurrentInsanity { get; private set; }
    public float CurrentDisturbance { get; private set; }

    [SerializeField] PlayerController player;
    [SerializeField] private LanternSystem lantern;
    const float MINIMUM_DISTURBANCE = 50f;

    // reference your lamp item (use a boolean for now)
    // True when the lantern is on
    public bool HasLight => lantern != null && lantern.IsOn;

    public bool IsDisturbed => CurrentDisturbance < disturbanceThreshold;

    void Awake()
    {
        CurrentInsanity = maxInsanity;
        CurrentDisturbance = maxInsanity;
    }

    void Update()
    {
        if (!player.isHidden)
        {
            if (!HasLight || IsDisturbed)
            {
                ModifyInsanity(-drainRate * Time.deltaTime);
            }
            else
            {
                ModifyInsanity(recoveryRate * Time.deltaTime);
            }
        }
        // allow sanity to recuperate if the player is hidden
        else
        {
            ModifyInsanity(recoveryRate * Time.deltaTime);
        }

        // reduce player disturbance
        if (player.IsSafeToCalm())
        {
            ModifyDisturbance(disturbanceRecoveryRate * Time.deltaTime);
        }
        else
        {
            ModifyDisturbance(-disturbanceDrainRate * Time.deltaTime);
        }

        Debug.Log($"Insanity: {CurrentInsanity:F1}, Disturbance: {CurrentDisturbance:F1}, HasLight: {HasLight}, Hidden: {player.isHidden}, Disturbed: {IsDisturbed}");
    }

    public void ModifyInsanity(float amount)
    {
        CurrentInsanity += amount;
        CurrentInsanity = Mathf.Clamp(CurrentInsanity, 0f, maxInsanity);
    }

    public void ModifyDisturbance(float amount)
    {
        CurrentDisturbance += amount;
        CurrentDisturbance = Mathf.Clamp(CurrentDisturbance, MINIMUM_DISTURBANCE, maxInsanity);
    }

    public void ResetSanity()
    {
        // sanity starts off at 100 and decreases from there (Sorry for the poor naming)
        CurrentInsanity = 100f;
        // Same thing for disturbance, 100 means not disturbed
        CurrentDisturbance = 100f;
    }
}
