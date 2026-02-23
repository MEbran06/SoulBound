using UnityEngine;

public class InsanitySystem : MonoBehaviour
{
    [Header("Insanity Settings")]
    public float maxInsanity = 100f;
    public float drainRate = 1f;
    public float recoveryRate = 3f;
    public float disturbanceThreshold = 50f;
    public float disturbaceRecoveryRate = 1f;

    public float CurrentInsanity { get; private set; }
    public float CurrentDisturbance { get; private set; }

    // reference your lamp item (use a boolean for now)
    [SerializeField] bool HasLight = false;

    public void Disturb()
    {
        CurrentDisturbance = CurrentDisturbance - (drainRate*Time.deltaTime);
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
        if (!HasLight || IsDisturbed)
        {
            ModifyInsanity(-drainRate * Time.deltaTime);
        }
        else
        {
            ModifyInsanity(recoveryRate * Time.deltaTime);
        }
    }

    public void ModifyInsanity(float amount)
    {
        CurrentInsanity += amount;
        CurrentInsanity = Mathf.Clamp(CurrentInsanity, 0f, maxInsanity);
    }
}
