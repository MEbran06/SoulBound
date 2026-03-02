using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerNoiseEmitter : MonoBehaviour
{
    private enum MoveMode {None, Walk, Sprint, Crouch }
    private MoveMode currentMode = MoveMode.Walk;

    private Vector3 lastPos;
    private float distAccum;

    public float walkStepDistance = 1.6f;
    public float sprintStepDistance = 1.3f;
    public float crouchStepDistance = 2.0f;

    [Range(0f, 1f)] public float walkLoudness = 0.35f;
    [Range(0f, 1f)] public float sprintLoudness = 0.75f;
    [Range(0f, 1f)] public float crouchLoudness = 0.15f;

    [Header("Movement Gate")]
    public float minSpeedToStep = 0.2f;

    [Header("Footstep Audio")]
    public AudioClip[] footstepClips;
    [Range(0f, 1f)] public float walkVolume = 0.7f;
    [Range(0f, 1f)] public float sprintVolume = 1.0f;
    [Range(0f, 1f)] public float crouchVolume = 0.35f;
    public float minPitch = 0.92f;
    public float maxPitch = 1.08f;

    [Header("References")]
    public CharacterController controller;
    public PlayerController playerController;

    private AudioSource audioSource;
    private float nextStepTime;

    void Reset()
    {
        controller = GetComponent<CharacterController>();
        playerController = GetComponent<PlayerController>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        lastPos = transform.position;
    }

    void Awake()
    {
        if (!controller) controller = GetComponent<CharacterController>();
        if (!playerController) playerController = GetComponent<PlayerController>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!controller) return;
        if (!controller.isGrounded) { lastPos = transform.position; return; }

        // horizontal delta distance this frame
        Vector3 pos = transform.position;
        Vector3 delta = pos - lastPos;
        delta.y = 0f;

        float moved = delta.magnitude;
        lastPos = pos;

        bool crouching = playerController != null && playerController.isCrouching;
        bool sprinting = playerController != null && playerController.isSprinting;

        // If not moving enough, don't accumulate
        if (moved < 0.001f) return;

        distAccum += moved;

        float loudness;
        float stepDist;
        float volume;

        if (crouching)
        {
            loudness = crouchLoudness;
            stepDist = crouchStepDistance;
            volume = crouchVolume;
        }
        else if (sprinting)
        {
            loudness = sprintLoudness;
            stepDist = sprintStepDistance;
            volume = sprintVolume;
        }
        else
        {
            loudness = walkLoudness;
            stepDist = walkStepDistance;
            volume = walkVolume;
        }

        // If we switched mode, resync so it feels responsive
        MoveMode newMode = crouching ? MoveMode.Crouch : (sprinting ? MoveMode.Sprint : MoveMode.Walk);
        if (newMode != currentMode)
        {
            currentMode = newMode;
            distAccum = 0f;
        }

        // Fire steps only when we "walked" a stride length
        if (distAccum >= stepDist)
        {
            distAccum -= stepDist; // keeps cadence stable even if framerate varies

            NoiseSystem.Emit(transform.position, loudness);

            if (!GameManager.Instance.isGameOver)
                PlayFootstep(volume);
        }
    }

    private void PlayFootstep(float volume)
    {
        if (footstepClips == null || footstepClips.Length == 0) return;

        var clip = footstepClips[Random.Range(0, footstepClips.Length)];
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(clip, volume);
    }
}