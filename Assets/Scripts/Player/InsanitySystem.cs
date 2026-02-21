using UnityEngine;

public class InsanitySystem : MonoBehaviour
{
    [Header("Insanity Settings")]
    [SerializeField] float maxInsanity = 100f;
    [SerializeField] float drainRate = 10f;
    [SerializeField] float recoveryRate = 5f;

    public float CurrentInsanity { get; private set; }

    // reference your lamp item (use a boolean for now)
    [SerializeField] bool HasLight = false;

    void Awake()
    {
        CurrentInsanity = maxInsanity;
    }

    void Update()
    {
        if (!HasLight)
        {
            ModifyInsanity(-drainRate * Time.deltaTime);
        }
        else
        {
            ModifyInsanity(recoveryRate * Time.deltaTime);
        }
        if (CurrentInsanity == 0f) Debug.Log("Intensity is now 0");
    }

    public void ModifyInsanity(float amount)
    {
        CurrentInsanity += amount;
        CurrentInsanity = Mathf.Clamp(CurrentInsanity, 0f, maxInsanity);
    }

    public float Normalized => CurrentInsanity / maxInsanity;
}
