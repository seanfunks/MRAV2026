using UnityEngine;
using UnityEngine.Splines;

public class SimpleFollowSpline : MonoBehaviour
{
    public SplineContainer splineContainer; // Reference to the SplineContainer component
    public float speed = 5f; // Speed of movement along the spline
    private float progress = 0f; // Progress along the spline as a value between 0 and 1

    private Vector3[] worldKnots;

    void Start()
    {
        // Convert local spline positions to world positions
        int knotCount = splineContainer.Spline.Count;
        worldKnots = new Vector3[knotCount];

        for (int i = 0; i < knotCount; i++)
        {
            worldKnots[i] = splineContainer.transform.TransformPoint(splineContainer.Spline[i].Position);
        }
    }

    void Update()
    {
        if (splineContainer == null || splineContainer.Spline == null)
        {
            Debug.LogError("SplineContainer is not assigned or initialized.");
            return;
        }

        // Calculate the progress based on speed and time
        progress += (speed * Time.deltaTime) / splineContainer.Spline.GetLength();

        // Loop the progress if it exceeds 1 to loop back to the start if needed
        if (progress > 1f)
        {
            progress = 0f;
        }

        // Evaluate the position on the spline based on the current progress
        Vector3 position = EvaluateSplinePosition(progress);

        // Set the object's position
        transform.position = position;

        // The object will maintain its original rotation (always facing forward)
    }

    public Vector3 GetCurrentPosition()
    {
        return EvaluateSplinePosition(progress);
    }

    private Vector3 EvaluateSplinePosition(float t)
    {
        int count = worldKnots.Length;
        if (count < 2)
        {
            return worldKnots.Length > 0 ? worldKnots[0] : Vector3.zero;
        }

        int segment = Mathf.Min(Mathf.FloorToInt(t * (count - 1)), count - 2);
        float segmentT = (t * (count - 1)) - segment;

        Vector3 p0 = worldKnots[Mathf.Max(segment - 1, 0)];
        Vector3 p1 = worldKnots[segment];
        Vector3 p2 = worldKnots[segment + 1];
        Vector3 p3 = worldKnots[Mathf.Min(segment + 2, count - 1)];

        return CatmullRomInterpolate(p0, p1, p2, p3, segmentT);
    }

    private Vector3 CatmullRomInterpolate(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            (2.0f * p1) +
            (-p0 + p2) * t +
            (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * t2 +
            (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * t3
        );
    }
}
