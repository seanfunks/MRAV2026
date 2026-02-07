using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scene-level manager that owns all bird groups, spawns their birds,
/// routes input from BirdInputMapper to the correct group, and updates
/// each group's active behavior every frame.
///
/// Setup:
/// 1. Create "BirdOrchestrator" GameObject in scene.
/// 2. Attach this + BirdInputMapper.
/// 3. Assign cameraTransform (Main Camera) and birdPrefab (eagle prefab).
/// 4. Hit Play.
/// </summary>
public class BirdOrchestrator : MonoBehaviour
{
    [Header("References")]
    public BirdInputMapper inputMapper;
    public Transform cameraTransform;
    public GameObject birdPrefab;

    [Header("FlapperBuddies (Group 1)")]
    public int flapperBuddiesCount = 2;
    public float flapperBuddiesScale = 2.0f;
    [Tooltip("Distance in front of camera")]
    public float flapperDistance = 10f;
    [Tooltip("Horizontal spread (left/right offset)")]
    public float flapperSpread = 3f;
    [Tooltip("Vertical offset above camera")]
    public float flapperHeight = 1.5f;

    // Groups
    private BirdGroup flapperBuddies;
    private BirdGroup circlingFlappers;
    private BirdGroup flockPatterns;
    private BirdGroup visualEffect;
    private BirdGroup[] allGroups;

    // Action ID → (groupIndex, localButton) mapping
    // Group 1 (FlapperBuddies):  keys 1,2,Q,W → actionIDs 0,1,4,5 → localButtons 0,1,2,3
    // Group 2 (CirclingFlappers): keys 3,4,E,R → actionIDs 2,3,6,7 → localButtons 0,1,2,3
    // Group 3 (FlockPatterns):    keys A,S,Z,X → actionIDs 8,9,12,13 → localButtons 0,1,2,3
    // Group 4 (VisualEffect):     keys D,F,C,V → actionIDs 10,11,14,15 → localButtons 0,1,2,3
    private int[] actionToGroup = new int[16];
    private int[] actionToLocal = new int[16];

    void Start()
    {
        // Initialize groups
        flapperBuddies = new BirdGroup { groupName = "FlapperBuddies" };
        circlingFlappers = new BirdGroup { groupName = "CirclingFlappers" };
        flockPatterns = new BirdGroup { groupName = "FlockPatterns" };
        visualEffect = new BirdGroup { groupName = "VisualEffect" };
        allGroups = new BirdGroup[] { flapperBuddies, circlingFlappers, flockPatterns, visualEffect };

        // Build action ID → group/local mapping
        SetupActionMapping();

        // Subscribe to input
        if (inputMapper != null)
        {
            inputMapper.OnActionEvent += OnActionEvent;
        }

        // Spawn birds for each active group
        SpawnFlapperBuddies();
        // CirclingFlappers and FlockPatterns will be spawned here when implemented
    }

    private void SetupActionMapping()
    {
        // Group 1 (FlapperBuddies): actionIDs 0,1,4,5
        actionToGroup[0] = 0; actionToLocal[0] = 0; // key 1
        actionToGroup[1] = 0; actionToLocal[1] = 1; // key 2
        actionToGroup[4] = 0; actionToLocal[4] = 2; // key Q
        actionToGroup[5] = 0; actionToLocal[5] = 3; // key W

        // Group 2 (CirclingFlappers): actionIDs 2,3,6,7
        actionToGroup[2] = 1; actionToLocal[2] = 0; // key 3
        actionToGroup[3] = 1; actionToLocal[3] = 1; // key 4
        actionToGroup[6] = 1; actionToLocal[6] = 2; // key E
        actionToGroup[7] = 1; actionToLocal[7] = 3; // key R

        // Group 3 (FlockPatterns): actionIDs 8,9,12,13
        actionToGroup[8]  = 2; actionToLocal[8]  = 0; // key A
        actionToGroup[9]  = 2; actionToLocal[9]  = 1; // key S
        actionToGroup[12] = 2; actionToLocal[12] = 2; // key Z
        actionToGroup[13] = 2; actionToLocal[13] = 3; // key X

        // Group 4 (VisualEffect): actionIDs 10,11,14,15
        actionToGroup[10] = 3; actionToLocal[10] = 0; // key D
        actionToGroup[11] = 3; actionToLocal[11] = 1; // key F
        actionToGroup[14] = 3; actionToLocal[14] = 2; // key C
        actionToGroup[15] = 3; actionToLocal[15] = 3; // key V
    }

    private void OnActionEvent(int actionId, bool pressed)
    {
        if (actionId < 0 || actionId >= 16) return;

        int groupIdx = actionToGroup[actionId];
        int localBtn = actionToLocal[actionId];

        if (pressed)
            allGroups[groupIdx].ButtonPressed(localBtn);
        else
            allGroups[groupIdx].ButtonReleased(localBtn);
    }

    // =====================================================
    // FLAPPER BUDDIES — Spawn & Behavior
    // =====================================================

    private void SpawnFlapperBuddies()
    {
        if (birdPrefab == null || flapperBuddiesCount <= 0) return;

        Vector3 spawnPos = cameraTransform != null
            ? cameraTransform.position + cameraTransform.forward * flapperDistance
            : transform.position;

        for (int i = 0; i < flapperBuddiesCount; i++)
        {
            // Spread left/right
            float side = (i % 2 == 0) ? -1f : 1f;
            Vector3 offset = (cameraTransform != null)
                ? cameraTransform.right * side * flapperSpread
                : Vector3.right * side * flapperSpread;

            GameObject bird = Instantiate(birdPrefab, spawnPos + offset, Quaternion.identity);
            bird.SetActive(true);
            bird.name = $"FlapperBuddy_{i}";
            bird.transform.localScale = Vector3.one * flapperBuddiesScale;

            // Disable flight physics — FlapperBuddies are directly positioned relative to camera
            var physics = bird.GetComponent<BirdFlightPhysics>();
            if (physics != null) physics.enabled = false;
            var animator = bird.GetComponent<BirdFlightAnimator>();
            if (animator != null) animator.enabled = false;

            flapperBuddies.RegisterBird(bird.transform);
        }

        Debug.Log($"Spawned {flapperBuddiesCount} FlapperBuddies (scale {flapperBuddiesScale}x)");
    }

    void Update()
    {
        UpdateFlapperBuddies();
        // UpdateCirclingFlappers(); — future
        // UpdateFlockPatterns(); — future
    }

    /// <summary>
    /// FlapperBuddies default: hover in front of camera, always facing away from user.
    /// Button behaviors override this when active.
    /// </summary>
    private void UpdateFlapperBuddies()
    {
        if (cameraTransform == null) return;

        int activeBtn = flapperBuddies.ActiveButton;

        if (activeBtn == -1)
        {
            // DEFAULT: hover near camera, facing away
            UpdateFlapperBuddiesDefault();
        }
        else
        {
            // Button behavior — placeholder for now
            // Will be filled in one at a time
            switch (activeBtn)
            {
                case 0: FlapperBuddiesBehavior0(); break;
                case 1: FlapperBuddiesBehavior1(); break;
                case 2: FlapperBuddiesBehavior2(); break;
                case 3: FlapperBuddiesBehavior3(); break;
            }
        }
    }

    private void UpdateFlapperBuddiesDefault()
    {
        float bob = Mathf.Sin(Time.time * 1.5f) * 0.3f; // gentle vertical bob

        for (int i = 0; i < flapperBuddies.birds.Count; i++)
        {
            if (flapperBuddies.birds[i] == null) continue;

            // Position: locked relative to camera — in front, spread left/right
            float side = (i % 2 == 0) ? -1f : 1f;
            // Each bird bobs slightly out of sync
            float birdBob = Mathf.Sin(Time.time * 1.5f + i * 1.2f) * 0.3f;

            Vector3 targetPos = cameraTransform.position
                + cameraTransform.forward * flapperDistance
                + cameraTransform.right * side * flapperSpread
                + cameraTransform.up * (flapperHeight + birdBob);

            // Smooth follow so they don't feel rigidly attached
            flapperBuddies.birds[i].position = Vector3.Lerp(
                flapperBuddies.birds[i].position,
                targetPos,
                8f * Time.deltaTime
            );

            // Face away from user (same direction camera is looking)
            Quaternion awayRot = Quaternion.LookRotation(cameraTransform.forward, Vector3.up);
            flapperBuddies.birds[i].rotation = Quaternion.Slerp(
                flapperBuddies.birds[i].rotation,
                awayRot,
                5f * Time.deltaTime
            );
        }
    }

    // --- FlapperBuddies button behaviors (placeholders) ---

    private void FlapperBuddiesBehavior0()
    {
        // Button 0 (key 1) — TBD
        UpdateFlapperBuddiesDefault(); // fallback to default until defined
    }

    private void FlapperBuddiesBehavior1()
    {
        // Button 1 (key 2) — TBD
        UpdateFlapperBuddiesDefault();
    }

    private void FlapperBuddiesBehavior2()
    {
        // Button 2 (key Q) — TBD
        UpdateFlapperBuddiesDefault();
    }

    private void FlapperBuddiesBehavior3()
    {
        // Button 3 (key W) — TBD
        UpdateFlapperBuddiesDefault();
    }

    void OnDestroy()
    {
        if (inputMapper != null)
            inputMapper.OnActionEvent -= OnActionEvent;
    }
}
