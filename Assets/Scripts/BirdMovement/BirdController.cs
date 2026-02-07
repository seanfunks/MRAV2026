using System.Collections;
using UnityEngine;

public class BirdController : MonoBehaviour
{
    public Transform player; // Reference to the player following the spline
    public SimpleFollowSpline playerSpline; // Reference to the SimpleFollowSpline script
    public Transform[] birdsA; // Birds that will circle around the player
    public Transform[] birdsB; // Birds that will circle in front of the player
    public Transform[] birdsTrigger; // Birds that will glide side to side
    public float flyToPositionSpeed = 3f; // Slower speed at which birds fly to their designated positions
    public float circleRadius = 4f; // Larger radius for the circling movement
    public float circleSpeed = 2f; // Speed of circling
    public float glideDistance = 4f; // Increase the distance for gliding side to side
    public float glideSpeed = 0.5f; // Slower speed of gliding
    public float forwardOffset = 1f; // Closer to the player
    public float leftOffset = 0.5f; // Amount to move birds to the left of the player
    public float downOffset = 0.5f; // Amount to move birds down relative to the player
    public float flyAwaySpeed = 0.2f; // Very slow speed at which birds fly away when the key is released
    public float returnSpeed = 0.2f; // Very slow speed at which birds return when flying back to start positions
    public float swayFrontOffset = 8f; // Slightly closer in front of the player
    public float glideSpacing = 5f; // Significantly increased spacing between gliding birds
    public float rotationSpeed = 2f; // Speed of rotation smoothing
    public float resetDistanceThreshold = 20f; // Distance at which birds reset to their starting position

    private Vector3[] targetPositions; // Array to store the target positions for each bird
    private bool[] isFlyingToPosition;
    private bool[] isFollowingPlayer;
    private bool[] isReturningToStart; // New state to track if birds are returning to start positions
    private bool[] isFlyingAway; // New state to track if birds are flying away
    private Vector3[] originalPositions; // Store original positions to return to

    private void Start()
    {
        int birdCount = birdsA.Length + birdsB.Length + birdsTrigger.Length;
        isFlyingToPosition = new bool[birdCount];
        isFollowingPlayer = new bool[birdCount];
        isReturningToStart = new bool[birdCount];
        isFlyingAway = new bool[birdCount];
        originalPositions = new Vector3[birdCount];
        targetPositions = new Vector3[birdCount];

        // Store the initial positions of the birds (relative to the player)
        int index = 0;
        for (int i = 0; i < birdsA.Length; i++, index++)
        {
            originalPositions[index] = birdsA[i].position;
            targetPositions[index] = new Vector3(3.29f - leftOffset, 1f - downOffset, 1f);
        }

        for (int i = 0; i < birdsB.Length; i++, index++)
        {
            originalPositions[index] = birdsB[i].position;
            targetPositions[index] = new Vector3(1f - leftOffset, 1f - downOffset, -0.04f);
        }

        for (int i = 0; i < birdsTrigger.Length; i++, index++)
        {
            originalPositions[index] = birdsTrigger[i].position;
            targetPositions[index] = new Vector3(-1.78f - leftOffset, 1f - downOffset, 1f);
        }
    }

    private void LateUpdate()
    {
        int birdIndex = 0;

        // Handle birds assigned to the F1 key (circle around the player)
        for (int i = 0; i < birdsA.Length; i++, birdIndex++)
        {
            HandleBirdMovement(birdsA[i], birdIndex, KeyCode.F1, CircleAroundPlayer);
        }

        // Handle birds assigned to the F2 key (circle in front of the player)
        for (int i = 0; i < birdsB.Length; i++, birdIndex++)
        {
            HandleBirdMovement(birdsB[i], birdIndex, KeyCode.F2, CircleInFrontOfPlayer);
        }

        // Handle birds assigned to the F3 key (glide side to side)
        for (int i = 0; i < birdsTrigger.Length; i++, birdIndex++)
        {
            HandleBirdMovement(birdsTrigger[i], birdIndex, KeyCode.F3, GlideSideToSide);
        }
    }

    private void HandleBirdMovement(Transform bird, int index, KeyCode key, System.Action<Transform, int> movementAction)
    {
        if (Input.GetKey(key))
        {
            if (isReturningToStart[index])
            {
                isReturningToStart[index] = false;
                isFlyingToPosition[index] = true;
            }

            if (!isFollowingPlayer[index] && !isReturningToStart[index])
            {
                MoveBirdToPosition(bird, targetPositions[index], index);
            }
            else if (isFollowingPlayer[index])
            {
                movementAction.Invoke(bird, index);
            }
        }
        else if (Input.GetKeyUp(key))
        {
            StartReturningToStartPosition(index);
        }
    }

    private void MoveBirdToPosition(Transform bird, Vector3 targetPosition, int index)
    {
        Vector3 worldTargetPosition = player.TransformPoint(targetPosition);
        bird.position = Vector3.MoveTowards(bird.position, worldTargetPosition, flyToPositionSpeed * Time.deltaTime);

        // Smoothly rotate the bird towards the movement direction
        Vector3 direction = (worldTargetPosition - bird.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            bird.rotation = Quaternion.Slerp(bird.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(bird.position, worldTargetPosition) < 0.1f)
        {
            isFlyingToPosition[index] = false;
            isFollowingPlayer[index] = true;
        }
    }

    private void CircleAroundPlayer(Transform bird, int index)
    {
        float angle = Time.time * circleSpeed + index * Mathf.PI * 2 / birdsA.Length;
        Vector3 offset = new Vector3(Mathf.Sin(angle) * circleRadius, 0, Mathf.Cos(angle) * circleRadius);
        bird.position = Vector3.Lerp(bird.position, player.position + offset, Time.deltaTime);

        // Make the bird face the direction of its movement along the circle
        Vector3 direction = (player.position - bird.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        bird.rotation = Quaternion.Slerp(bird.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void CircleInFrontOfPlayer(Transform bird, int index)
    {
        float angle = Time.time * circleSpeed + index * Mathf.PI * 2 / birdsB.Length;
        Vector3 frontPosition = player.position + player.forward * circleRadius * 2;
        Vector3 offset = new Vector3(Mathf.Sin(angle) * circleRadius, 0, Mathf.Cos(angle) * circleRadius);
        bird.position = Vector3.Lerp(bird.position, frontPosition + offset, Time.deltaTime);

        // Make the bird face the direction of its movement along the circle
        Vector3 direction = (frontPosition - bird.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        bird.rotation = Quaternion.Slerp(bird.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void GlideSideToSide(Transform bird, int index)
    {
        float glideOffset = Mathf.Sin(Time.time * glideSpeed) * glideDistance;
        Vector3 frontPosition = player.position + player.forward * swayFrontOffset; // Farther in front of the player
        bird.position = Vector3.Lerp(bird.position, frontPosition + new Vector3(glideOffset * (index + 1) * glideSpacing, 0, 0), Time.deltaTime);

        // Make the bird face the direction it is moving
        Vector3 direction = player.forward;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        bird.rotation = Quaternion.Slerp(bird.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void StartReturningToStartPosition(int index)
    {
        isFollowingPlayer[index] = false;
        isReturningToStart[index] = true;
        StartCoroutine(ReturnToStartCoroutine(index));
    }

    private IEnumerator ReturnToStartCoroutine(int index)
    {
        Transform bird = GetBirdTransformByIndex(index);
        Vector3 splinePosition = playerSpline.GetCurrentPosition();

        while (true)
        {
            bird.position = Vector3.MoveTowards(bird.position, splinePosition + originalPositions[index], returnSpeed * Time.deltaTime);

            // Ensure the bird faces the direction it is moving back to
            Vector3 direction = (splinePosition + originalPositions[index] - bird.position).normalized;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                bird.rotation = Quaternion.Slerp(bird.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            yield return null;

            // If the bird is far enough from the player, snap it back to its original position
            if (Vector3.Distance(bird.position, splinePosition + originalPositions[index]) < 0.1f || Vector3.Distance(bird.position, player.position) > resetDistanceThreshold)
            {
                bird.position = splinePosition + originalPositions[index];

                // Log the reset event
                Debug.Log($"Bird {index + 1} reset to its original position.");

                isFollowingPlayer[index] = true;
                isReturningToStart[index] = false;
                yield break;
            }

            // If the button is pressed again during the return, break the loop
            if (Input.GetKey(KeyCode.F1) && index < birdsA.Length ||
                Input.GetKey(KeyCode.F2) && index >= birdsA.Length && index < birdsA.Length + birdsB.Length ||
                Input.GetKey(KeyCode.F3) && index >= birdsA.Length + birdsB.Length)
            {
                isReturningToStart[index] = false;
                isFlyingToPosition[index] = true;
                yield break;
            }
        }
    }

    private Transform GetBirdTransformByIndex(int index)
    {
        if (index < birdsA.Length)
            return birdsA[index];
        else if (index < birdsA.Length + birdsB.Length)
            return birdsB[index - birdsA.Length];
        else
            return birdsTrigger[index - birdsA.Length - birdsB.Length];
    }
}
