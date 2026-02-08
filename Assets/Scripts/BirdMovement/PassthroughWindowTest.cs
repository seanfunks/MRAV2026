using UnityEngine;

/// <summary>
/// Feasibility test: spawns a world-locked passthrough window (quad) in front of the user.
/// Press P to toggle visibility. Requires OVRCameraRig + OVRPassthroughLayer in the scene.
/// </summary>
public class PassthroughWindowTest : MonoBehaviour
{
    [Header("Passthrough")]
    public OVRPassthroughLayer passthroughLayer;

    [Header("Window Settings")]
    public float windowWidth = 2f;
    public float windowHeight = 1.5f;
    public float spawnDistance = 3f;

    [Header("Material")]
    [Tooltip("Assign SelectivePassthrough.mat from StarterSamples/Usage/Passthrough/Materials")]
    public Material passthroughMaterial;

    private GameObject windowQuad;
    private bool windowVisible = true;

    void Start()
    {
        if (passthroughLayer == null)
        {
            Debug.LogError("PassthroughWindowTest: No OVRPassthroughLayer assigned!");
            return;
        }

        CreateWindow();
    }

    private void CreateWindow()
    {
        // Spawn position: in front of wherever the camera is at Start
        Transform cam = Camera.main != null ? Camera.main.transform : transform;
        Vector3 spawnPos = cam.position + cam.forward * spawnDistance;
        Quaternion spawnRot = Quaternion.LookRotation(cam.forward, Vector3.up);

        // Create quad
        windowQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        windowQuad.name = "PassthroughWindow";
        windowQuad.transform.position = spawnPos;
        windowQuad.transform.rotation = spawnRot;
        windowQuad.transform.localScale = new Vector3(windowWidth, windowHeight, 1f);

        // Remove collider (not needed)
        var col = windowQuad.GetComponent<Collider>();
        if (col != null) Destroy(col);

        // Apply passthrough material
        if (passthroughMaterial != null)
        {
            windowQuad.GetComponent<MeshRenderer>().material = passthroughMaterial;
        }

        // Register with passthrough layer as a projection surface
        MeshFilter mf = windowQuad.GetComponent<MeshFilter>();
        passthroughLayer.AddSurfaceGeometry(mf.gameObject, true);

        Debug.Log($"Passthrough window created at {spawnPos}, size {windowWidth}x{windowHeight}");
    }

    void Update()
    {
        // Toggle with P key
        if (Input.GetKeyDown(KeyCode.P))
        {
            windowVisible = !windowVisible;

            if (windowQuad != null)
            {
                if (windowVisible)
                {
                    windowQuad.SetActive(true);
                    MeshFilter mf = windowQuad.GetComponent<MeshFilter>();
                    passthroughLayer.AddSurfaceGeometry(mf.gameObject, true);
                    Debug.Log("Passthrough window: ON");
                }
                else
                {
                    passthroughLayer.RemoveSurfaceGeometry(windowQuad);
                    windowQuad.SetActive(false);
                    Debug.Log("Passthrough window: OFF");
                }
            }
        }
    }

    void OnDestroy()
    {
        if (windowQuad != null && passthroughLayer != null)
        {
            passthroughLayer.RemoveSurfaceGeometry(windowQuad);
            Destroy(windowQuad);
        }
    }
}
