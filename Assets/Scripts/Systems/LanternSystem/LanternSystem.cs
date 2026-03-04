using UnityEngine;

public class LanternSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light lanternLight; // drag a Light here (or child light)
    [SerializeField] private GameObject lanternVisual; // optional: model/mesh to show/hide

    [Header("Fuel")]
    [SerializeField] private float maxFuel = 100f;
    [SerializeField] private float drainPerSecond = 2f;

    public float Fuel { get; private set; }
    public bool IsOn { get; private set; }

    private void Awake()
    {
        Fuel = maxFuel;
        ApplyState();
    }

    private void Update()
    {
        if (!IsOn) return;

        if (Fuel > 0f)
        {
            Fuel -= drainPerSecond * Time.deltaTime;
            Fuel = Mathf.Clamp(Fuel, 0f, maxFuel);

            if (Fuel <= 0f)
            {
                IsOn = false;
                ApplyState();
            }
        }
    }

    public bool Toggle()
    {
        if (IsOn)
        {
            IsOn = false;
            ApplyState();
            return true;
        }

        // turning ON
        if (Fuel <= 0f) return false;

        IsOn = true;
        ApplyState();
        return true;
    }

    public void SetOn(bool on)
    {
        IsOn = on && Fuel > 0f;
        ApplyState();
    }

    public void AddFuel(float amount)
    {
        if (amount <= 0f) return;

        Fuel = Mathf.Clamp(Fuel + amount, 0f, maxFuel);

        // If it was off because it was empty, you can decide whether to auto-turn-on.
        // I’d keep it OFF until the player toggles.
        ApplyState();
    }

    private void ApplyState()
    {
        if (lanternLight != null)
            lanternLight.enabled = IsOn;

        if (lanternVisual != null)
            lanternVisual.SetActive(true); // keep model visible; change to IsOn if you want it hidden when off
    }
}