using UnityEngine;

/// <summary>
/// Per-bird flight physics. Attach to each eagle GameObject.
/// Simulates gravity, periodic lift from wing flaps, forward thrust,
/// aerodynamic drag, steering toward targets, and orientation with banking.
/// </summary>
public class BirdFlightPhysics : MonoBehaviour
{
    [Header("Gravity & Lift")]
    public float gravity = 9.81f;
    public float liftForce = 3.0f;
    public float flapFrequency = 1.5f;

    [Header("Propulsion")]
    public float thrustForce = 6.0f;
    public float horizontalDrag = 0.1f;
    public float verticalDrag = 0.02f;
    public float maxHorizontalSpeed = 15.0f;
    public float minSpeed = 2.0f;

    [Header("Steering")]
    public float steeringForce = 4.0f;

    [Header("Auto Flap/Glide Cycle")]
    [Tooltip("Min seconds of flapping before a glide")]
    public float flapDurationMin = 3.0f;
    [Tooltip("Max seconds of flapping before a glide")]
    public float flapDurationMax = 7.0f;
    [Tooltip("Min seconds of gliding before resuming flaps")]
    public float glideDurationMin = 5.0f;
    [Tooltip("Max seconds of gliding before resuming flaps")]
    public float glideDurationMax = 10.0f;

    [Header("Orientation")]
    public float bankAngleMax = 45.0f;
    public float pitchInfluence = 0.3f;
    public float rotationSmoothing = 5.0f;

    [Header("State (read-only)")]
    [SerializeField] private Vector3 velocity;
    [SerializeField] private float currentSpeed;
    [SerializeField] private float verticalVelocity;
    [SerializeField] private float bankAngle;
    [SerializeField] private float flapPhase;
    [SerializeField] private bool isFlapping = true;

    // Public accessors for BirdFlightAnimator
    public Vector3 Velocity => velocity;
    public float CurrentSpeed => currentSpeed;
    public float VerticalVelocity => verticalVelocity;
    public float BankAngle => bankAngle;
    public float FlapPhase => flapPhase;
    public bool IsFlapping => isFlapping;
    public float MinSpeed => minSpeed;
    public float MaxSpeed => maxHorizontalSpeed;

    // Steering target
    private Vector3 steeringTarget;
    private bool hasSteeringTarget;

    // Flap timing
    private float flapTimer;
    private float flapRandomOffset;

    // Auto flap/glide cycle
    private float cycleTimer;
    private float currentCycleDuration;

    void Start()
    {
        flapRandomOffset = Random.Range(0f, 10f);
        velocity = transform.forward * (minSpeed + maxHorizontalSpeed) * 0.5f;
        isFlapping = true;
        currentCycleDuration = Random.Range(flapDurationMin, flapDurationMax);
        cycleTimer = Random.Range(0f, currentCycleDuration); // stagger so birds don't all glide at once
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        // 0. Auto flap/glide cycle
        cycleTimer += dt;
        if (cycleTimer >= currentCycleDuration)
        {
            cycleTimer = 0f;
            isFlapping = !isFlapping;
            currentCycleDuration = isFlapping
                ? Random.Range(flapDurationMin, flapDurationMax)
                : Random.Range(glideDurationMin, glideDurationMax);
        }

        // 1. Gravity
        velocity.y -= gravity * dt;

        // 2. Lift: base lift counters gravity, flap cycle adds bobbing
        flapTimer += dt;
        float flapAngle = (flapTimer + flapRandomOffset) * flapFrequency * 2f * Mathf.PI;
        flapPhase = (Mathf.Sin(flapAngle) + 1f) * 0.5f; // 0..1 for animation

        // Lift always active (simulates wind/thermals during glide)
        // Base lift: fully counters gravity + slight surplus to climb
        velocity.y += (gravity + 1.0f) * dt;
        // Periodic bobbing: oscillates ±liftForce for natural motion
        velocity.y += Mathf.Sin(flapAngle) * liftForce * dt;

        // 3. Forward thrust
        velocity += transform.forward * thrustForce * dt;

        // 4. Drag — applied separately so vertical lift isn't crushed by horizontal speed
        // Horizontal drag (quadratic, based on horizontal speed only)
        Vector3 horizontalVel = new Vector3(velocity.x, 0f, velocity.z);
        float hSpeedSq = horizontalVel.sqrMagnitude;
        if (hSpeedSq > 0.01f)
        {
            Vector3 hDrag = -horizontalVel.normalized * horizontalDrag * hSpeedSq;
            velocity.x += hDrag.x * dt;
            velocity.z += hDrag.z * dt;
        }

        // Vertical drag (light, so lift can actually work)
        velocity.y -= velocity.y * verticalDrag;

        // 5. Horizontal speed clamping (don't touch vertical component)
        float hSpeed = new Vector3(velocity.x, 0f, velocity.z).magnitude;
        if (hSpeed > maxHorizontalSpeed)
        {
            float scale = maxHorizontalSpeed / hSpeed;
            velocity.x *= scale;
            velocity.z *= scale;
        }
        else if (hSpeed > 0.01f && hSpeed < minSpeed)
        {
            float scale = minSpeed / hSpeed;
            velocity.x *= scale;
            velocity.z *= scale;
        }
        currentSpeed = velocity.magnitude;

        // 6. Steering toward target
        if (hasSteeringTarget)
        {
            Vector3 toTarget = steeringTarget - transform.position;
            if (toTarget.sqrMagnitude > 1f) // don't steer when very close
            {
                Vector3 desired = toTarget.normalized * currentSpeed;
                Vector3 steer = (desired - velocity).normalized * steeringForce;
                velocity += steer * dt;
            }
        }

        // 7. Position integration
        transform.position += velocity * dt;

        // 8. Orientation: face velocity with pitch and bank
        if (currentSpeed > 0.1f)
        {
            // Primary: face velocity direction
            Quaternion targetRot = Quaternion.LookRotation(velocity.normalized, Vector3.up);

            // Bank/roll: based on lateral steering component
            Vector3 localVel = transform.InverseTransformDirection(velocity);
            float lateralDelta = localVel.x;
            bankAngle = Mathf.Clamp(-lateralDelta * bankAngleMax / (currentSpeed + 0.1f),
                                     -bankAngleMax, bankAngleMax);
            Quaternion bankRot = Quaternion.Euler(0f, 0f, bankAngle);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot * bankRot,
                                                   rotationSmoothing * dt);
        }

        // 9. Record vertical velocity for animation
        verticalVelocity = velocity.y;
    }

    // --- Public API ---

    public void SetSteeringTarget(Vector3 target)
    {
        steeringTarget = target;
        hasSteeringTarget = true;
    }

    public void ClearSteeringTarget()
    {
        hasSteeringTarget = false;
    }

    public void SetFlapping(bool flap)
    {
        isFlapping = flap;
    }

    public void ApplyImpulse(Vector3 impulse)
    {
        velocity += impulse;
    }

    public void InitializeVelocity(Vector3 v)
    {
        velocity = v;
    }
}
