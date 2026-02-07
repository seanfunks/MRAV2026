using UnityEngine;

/// <summary>
/// Per-bird animation driver. Reads BirdFlightPhysics state and
/// drives the Eagle Animator Controller (Eagle_Controller RG1).
/// Animator parameters: GlideABit, FlyABit, Dive (triggers), FlappySpeedAdjust (float).
/// States: Fly (default), Glide, Falling, eagleWingTuck.
/// </summary>
[RequireComponent(typeof(BirdFlightPhysics))]
public class BirdFlightAnimator : MonoBehaviour
{
    [Header("Thresholds")]
    public float glideSpeedThreshold = 8.0f;
    public float diveVerticalThreshold = -5.0f;
    public float recoverVerticalThreshold = -2.0f;

    [Header("Animation Speed Mapping")]
    public float flapSpeedMin = 0.5f;
    public float flapSpeedMax = 2.0f;

    private Animator animator;
    private BirdFlightPhysics physics;

    private enum FlightAnimState { Fly, Glide, Falling, WingTuck }
    private FlightAnimState currentState = FlightAnimState.Fly;

    // Animator parameter hashes (cached for performance)
    private static readonly int HashGlideABit = Animator.StringToHash("GlideABit");
    private static readonly int HashFlyABit = Animator.StringToHash("FlyABit");
    private static readonly int HashDive = Animator.StringToHash("Dive");
    private static readonly int HashFlappySpeedAdjust = Animator.StringToHash("FlappySpeedAdjust");

    void Start()
    {
        animator = GetComponent<Animator>();
        physics = GetComponent<BirdFlightPhysics>();
    }

    void Update()
    {
        if (animator == null || physics == null) return;

        // Continuously update flap animation speed based on flight speed
        float speedNorm = Mathf.InverseLerp(physics.MinSpeed, physics.MaxSpeed, physics.CurrentSpeed);
        animator.SetFloat(HashFlappySpeedAdjust, Mathf.Lerp(flapSpeedMin, flapSpeedMax, speedNorm));

        // Determine desired animation state from physics
        FlightAnimState desired = DetermineDesiredState();
        if (desired != currentState)
        {
            TransitionTo(desired);
        }
    }

    private FlightAnimState DetermineDesiredState()
    {
        float vy = physics.VerticalVelocity;
        float speed = physics.CurrentSpeed;
        bool flapping = physics.IsFlapping;

        // Priority: dive first (strong downward motion)
        if (vy < diveVerticalThreshold)
            return FlightAnimState.Falling;

        // Recovering from a fall
        if (currentState == FlightAnimState.Falling && vy > recoverVerticalThreshold)
            return FlightAnimState.Glide; // recover through glide first

        // Gliding: not flapping and fast enough
        if (!flapping && speed > glideSpeedThreshold)
            return FlightAnimState.Glide;

        // Default: flapping flight
        if (flapping)
            return FlightAnimState.Fly;

        return currentState; // no change
    }

    private void TransitionTo(FlightAnimState newState)
    {
        switch (newState)
        {
            case FlightAnimState.Fly:
                animator.SetTrigger(HashFlyABit);
                break;
            case FlightAnimState.Glide:
                animator.SetTrigger(HashGlideABit);
                break;
            case FlightAnimState.Falling:
                animator.SetTrigger(HashDive);
                break;
            case FlightAnimState.WingTuck:
                animator.Play("eagleWingTuck");
                break;
        }
        currentState = newState;
    }

    // --- Public API for external control ---

    public void ForceWingTuck()
    {
        TransitionTo(FlightAnimState.WingTuck);
        physics.SetFlapping(false);
    }

    public void ForceFlap()
    {
        TransitionTo(FlightAnimState.Fly);
        physics.SetFlapping(true);
    }
}
