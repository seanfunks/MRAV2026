using UnityEngine;

/// <summary>
/// Hold right mouse button + move mouse to look around (pan/tilt).
/// Does NOT move the camera position — rotation only.
/// Attach to the Camera GameObject.
/// </summary>
public class CameraMouseLook : MonoBehaviour
{
    public float sensitivity = 2.0f;

    private float yaw;
    private float pitch;

    void Start()
    {
        Vector3 euler = transform.eulerAngles;
        yaw = euler.y;
        pitch = euler.x;
    }

    void Update()
    {
        if (Input.GetMouseButton(1)) // right mouse button held
        {
            yaw += Input.GetAxis("Mouse X") * sensitivity;
            pitch -= Input.GetAxis("Mouse Y") * sensitivity;
            pitch = Mathf.Clamp(pitch, -90f, 90f);

            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }
    }
}
