using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scene-level flock manager. Spawns birds from a prefab at runtime,
/// provides default flocking behavior, and handles the toggle between
/// the old BirdController system and the new physics system.
///
/// Setup:
/// 1. Create eagle prefab: SK_Eagle model + Animator (Eagle_Controller RG1)
///    + BirdFlightPhysics + BirdFlightAnimator. Save as prefab. Disable the prefab in scene.
/// 2. Create "BirdFlightManager" GameObject, attach this + BirdInputMapper.
/// 3. Assign birdPrefab, spawnCount, player reference.
/// 4. Hit Play — birds spawn and fly.
/// </summary>
public class BirdFlightController : MonoBehaviour
{
    [Header("System Toggle")]
    [Tooltip("True = new physics flight, False = old BirdController paths")]
    public bool useNewFlightSystem = true;
    public BirdController oldBirdController;

    [Header("Spawning")]
    [Tooltip("Prefab with BirdFlightPhysics + BirdFlightAnimator + Animator already attached")]
    public GameObject birdPrefab;
    public int spawnCount = 5;
    [Tooltip("Area around spawn origin where birds appear")]
    public float spawnRadius = 10f;
    [Tooltip("Height at which birds spawn")]
    public float spawnHeight = 8f;

    [Header("References")]
    public Transform player;
    public SimpleFollowSpline playerSpline;

    [Header("Default Flocking")]
    [Tooltip("Offset from player where the flock center orbits")]
    public Vector3 flockCenterOffset = new Vector3(0f, 3f, 10f);
    public float flockSpreadRadius = 5f;
    public float orbitSpeed = 0.3f;
    [Tooltip("Vertical variation amplitude for spread positions")]
    public float verticalSpread = 1.5f;

    // Runtime lists (populated by spawning or manual assignment)
    private List<Transform> birds = new List<Transform>();
    private List<BirdFlightPhysics> birdPhysics = new List<BirdFlightPhysics>();
    private List<BirdFlightAnimator> birdAnimators = new List<BirdFlightAnimator>();
    private List<Vector3> homePositions = new List<Vector3>();

    void Start()
    {
        // Spawn birds from prefab
        if (birdPrefab != null && spawnCount > 0)
        {
            SpawnFlock();
        }

        ApplySystemToggle();
    }

    private void SpawnFlock()
    {
        Vector3 origin = player != null ? player.position : transform.position;

        for (int i = 0; i < spawnCount; i++)
        {
            // Spread birds in a circle at spawn height
            float angle = (2f * Mathf.PI * i) / spawnCount;
            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * spawnRadius,
                spawnHeight + Random.Range(-1f, 1f),
                Mathf.Sin(angle) * spawnRadius
            );
            Vector3 spawnPos = origin + offset;

            // Face toward center
            Quaternion spawnRot = Quaternion.LookRotation((origin + Vector3.up * spawnHeight) - spawnPos);

            GameObject bird = Instantiate(birdPrefab, spawnPos, spawnRot);
            bird.SetActive(true);
            bird.name = $"Eagle_{i}";

            RegisterBird(bird.transform);
        }

        Debug.Log($"Spawned {spawnCount} birds from prefab.");
    }

    /// <summary>
    /// Register an existing bird (e.g., one already in the scene) with the flock manager.
    /// </summary>
    public void RegisterBird(Transform bird)
    {
        birds.Add(bird);
        birdPhysics.Add(bird.GetComponent<BirdFlightPhysics>());
        birdAnimators.Add(bird.GetComponent<BirdFlightAnimator>());
        homePositions.Add(bird.position);
    }

    void Update()
    {
        // Toggle key
        if (Input.GetKeyDown(KeyCode.F12))
        {
            ToggleSystem();
            Debug.Log($"Bird flight system toggled. New system: {useNewFlightSystem}");
        }

        if (!useNewFlightSystem) return;

        UpdateDefaultFlocking();
    }

    /// <summary>
    /// Default behavior: each bird steers toward a unique point
    /// spread around a flock center that orbits near the player.
    /// </summary>
    private void UpdateDefaultFlocking()
    {
        if (player == null) return;

        Vector3 flockCenter = player.position + player.TransformDirection(flockCenterOffset);

        for (int i = 0; i < birds.Count; i++)
        {
            if (birdPhysics[i] == null || !birdPhysics[i].enabled) continue;

            float angle = (2f * Mathf.PI * i) / birds.Count + Time.time * orbitSpeed;
            Vector3 spreadOffset = new Vector3(
                Mathf.Cos(angle) * flockSpreadRadius,
                Mathf.Sin(angle * 0.7f) * verticalSpread,
                Mathf.Sin(angle) * flockSpreadRadius
            );

            birdPhysics[i].SetSteeringTarget(flockCenter + spreadOffset);
        }
    }

    // --- System Toggle ---

    public void ToggleSystem()
    {
        useNewFlightSystem = !useNewFlightSystem;
        ApplySystemToggle();
    }

    private void ApplySystemToggle()
    {
        // Old system
        if (oldBirdController != null)
            oldBirdController.enabled = !useNewFlightSystem;

        // Per-bird components
        for (int i = 0; i < birds.Count; i++)
        {
            if (birdPhysics[i] != null)
                birdPhysics[i].enabled = useNewFlightSystem;
            if (birdAnimators[i] != null)
                birdAnimators[i].enabled = useNewFlightSystem;

            // Disable old per-bird scripts if present
            var oldPath = birds[i].GetComponent<birdPath>();
            if (oldPath != null) oldPath.enabled = !useNewFlightSystem;
            var oldSpin = birds[i].GetComponent<BirdSpinning>();
            if (oldSpin != null) oldSpin.enabled = !useNewFlightSystem;
        }
    }

    // --- Flock Commands (called by BirdInputMapper or external systems) ---

    public int BirdCount => birds.Count;

    public void SetAllFlapping(bool flap)
    {
        foreach (var p in birdPhysics)
            if (p != null) p.SetFlapping(flap);
    }

    public void ApplyFlockImpulse(Vector3 impulse)
    {
        foreach (var p in birdPhysics)
            if (p != null) p.ApplyImpulse(impulse);
    }

    public void SetFlockTarget(Vector3 position)
    {
        foreach (var p in birdPhysics)
            if (p != null) p.SetSteeringTarget(position);
    }

    public void ReturnAllToHome()
    {
        for (int i = 0; i < birds.Count; i++)
            if (birdPhysics[i] != null) birdPhysics[i].SetSteeringTarget(homePositions[i]);
    }

    public void TriggerDiveAll()
    {
        foreach (var p in birdPhysics)
            if (p != null) p.ApplyImpulse(Vector3.down * 15f);
    }

    public void TriggerWingTuckAll()
    {
        foreach (var a in birdAnimators)
            if (a != null) a.ForceWingTuck();
    }

    public void TriggerFlapAll()
    {
        foreach (var a in birdAnimators)
            if (a != null) a.ForceFlap();
    }
}
