using UnityEngine;

public class CameraManager : MonoBehaviour
{
    #region Public Fields

    public bool allowRotation = true;
    public bool lockCursor = true;
    public bool returnToOrigin = true;
    public bool rotateObjects = true;
    public bool stayBehindTarget = false;
    public float distance = 5;
    public float maxAngle = 90;
    public float maxDistance = 10;
    public float minAngle = -90;
    public float minDistance = 0;
    public float returnSmoothing = 3;
    public float zoomSmooth = 16;
    public float zoomSpeed = 1;
    public LayerMask collisionLayers = new LayerMask();
    public static bool invertX, invertY;
    public static Vector2 sensitivity;
    public Transform target;
    public Vector2 originRotation = new Vector2();
    public Vector2 targetOffset = new Vector2();

    #endregion Public Fields

    #region Private Fields
    private float newDistance;
    private Quaternion rotation;
    private Vector2 inputRotation;

    #endregion Private Fields

    #region Public Methods

    public static void SetInvertX(bool invert)
    {
        invertX = invert;
    }

    public static void SetInvertY(bool invert)
    {
        invertY = invert;
    }

    public static void SetSensitivity(float sens)
    {
        sensitivity = new Vector2(sens, sens);
    }

    #endregion Public Methods

    #region Private Methods

    // Limit rotation.
    private void ClampRotation()
    {
        if (originRotation.x < -180)
            originRotation.x += 360;
        else if (originRotation.x > 180)
            originRotation.x -= 360;

        if (inputRotation.x - originRotation.x < -180)
            inputRotation.x += 360;
        else if (inputRotation.x - originRotation.x > 180)
            inputRotation.x -= 360;
    }

    private void LateUpdate()
    {
        if (target)
        {
            Zoom();
            Movement();
        }
    }

    private void Movement()
    {
        // Change position depending on mouse drag.
        if (allowRotation && (Input.GetMouseButton(0) || Input.GetMouseButton(1)))
        {
            if (invertX)
                inputRotation.x -= Input.GetAxis("Mouse X") * sensitivity.x;
            else
                inputRotation.x += Input.GetAxis("Mouse X") * sensitivity.x;

            ClampRotation();

            if (invertY)
                inputRotation.y += Input.GetAxis("Mouse Y") * sensitivity.y;
            else
                inputRotation.y -= Input.GetAxis("Mouse Y") * sensitivity.y;

            inputRotation.y = Mathf.Clamp(inputRotation.y, minAngle, maxAngle);

            rotation = Quaternion.Euler(inputRotation.y, inputRotation.x, 0);

            // Reset position if right button is used.
            if (Input.GetMouseButton(1))
            {
                originRotation = inputRotation;
                ClampRotation();
            }
        }
        else
        {
            // Keep camera behind target.
            if (stayBehindTarget)
            {
                originRotation.x = target.eulerAngles.y;
                ClampRotation();
            }

            // Move camera to default position.
            if (returnToOrigin)
                inputRotation = Vector3.Lerp(inputRotation, originRotation, returnSmoothing * Time.deltaTime);

            rotation = Quaternion.Euler(inputRotation.y, inputRotation.x, 0);
        }

        distance = Mathf.Clamp(Mathf.Lerp(distance, newDistance, Time.deltaTime * zoomSmooth), minDistance, maxDistance);

        // Define position based on rotation and distance.
        Vector3 current_position = rotation * new Vector3(targetOffset.x, 0, 0) + target.position + new Vector3(0, targetOffset.y, 0);
        Vector3 wanted_position = rotation * new Vector3(targetOffset.x, 0, -newDistance - 0.2f) + target.position + new Vector3(0, targetOffset.y, 0);

        // Check if there are objects between the camera and the target (with collision layers).
        RaycastHit hit;

        if (Physics.Linecast(current_position, wanted_position, out hit, collisionLayers))
            distance = Vector3.Distance(current_position, hit.point) - 0.2f;

        // Set position and rotation.
        transform.position = rotation * new Vector3(targetOffset.x, 0.0f, -distance) + target.position + new Vector3(0, targetOffset.y, 0);
        transform.rotation = rotation;
    }

    private void Start()
    {
        sensitivity = new Vector2(3, 3);
        invertX = false;
        invertY = false;

        newDistance = distance;
        inputRotation = originRotation;
    }

    private void Update()
    {
        if (target)
        {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
                {
                    // Lock mouse cursor to center on movement.
                    if (Cursor.visible)
                    {
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                    }
                }
                return;
            }
        }

        if (!Cursor.visible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void Zoom()
    {
        // Zoom
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
            newDistance += zoomSpeed;
        else if (Input.GetAxis("Mouse ScrollWheel") > 0)
            newDistance -= zoomSpeed;

        newDistance = Mathf.Clamp(newDistance, minDistance, maxDistance);
    }

    #endregion Private Methods
}