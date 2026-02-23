using UnityEngine;

public class InsanitySystem : MonoBehaviour
{
    [Header("Insanity Settings")]
    public float maxInsanity = 100f;
    public float drainRate = 1f;
    public float recoveryRate = 3f;
    public float disturbanceThreshold = 50f;
    public float disturbanceRecoveryRate = 0.5f;
    public float disturbanceDrainRate = 5f;

    public float CurrentInsanity { get; private set; }
    public float CurrentDisturbance { get; private set; }

    // reference your lamp item (use a boolean for now)
    [SerializeField] bool HasLight = false;

    public void Disturb()
    {
        CurrentDisturbance = CurrentDisturbance - (disturbanceDrainRate*Time.deltaTime);
        CurrentDisturbance = Mathf.Clamp(CurrentDisturbance, 0f, maxInsanity);
    }

    public bool IsDisturbed => CurrentDisturbance < disturbanceThreshold;

    void Awake()
    {
        CurrentInsanity = maxInsanity;
        CurrentDisturbance = maxInsanity;
    }

    void Update()
    {
        Debug.Log($"Is disturbed: {IsDisturbed}, current disturbance: {CurrentDisturbance}");
        if (!HasLight || IsDisturbed)
        {
            ModifyInsanity(-drainRate * Time.deltaTime);
        }
        else
        {
            ModifyInsanity(recoveryRate * Time.deltaTime);
        }

        if (HasLight && IsDisturbed)
        {
            CurrentDisturbance = CurrentDisturbance + (disturbanceRecoveryRate*Time.deltaTime);
            CurrentDisturbance = Mathf.Clamp(CurrentDisturbance, 0f, maxInsanity);
        }
    }

    public void ModifyInsanity(float amount)
    {
        CurrentInsanity += amount;
        CurrentInsanity = Mathf.Clamp(CurrentInsanity, 0f, maxInsanity);
    }
}
