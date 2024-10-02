using UnityEngine;
using UnityEngine.EventSystems; // Add this for EventSystem

public class CameraController : MonoBehaviour
{
    public LayerMask uiLayerMask;
    public static CameraController instance;
    public Transform followTransform;
    public Transform cameraTransform;

    [Header("Movement Settings")]
    public float movementSpeed = 10f;
    public float movementTime = 5f;
    public float rotationAmount = 1f;
    public Vector3 zoomAmount = new Vector3(0, -5, 5);

    [Header("Zoom Limits")]
    public float minZoom = 10f;
    public float maxZoom = 100f;

    [Header("Boundary Settings")]
    public float boundaryWidth = 500f;

    private Vector3 newPosition;
    private Quaternion newRotation;
    private Vector3 newZoom;

    private Vector3 dragStartPosition;
    private Vector3 dragCurrentPosition;
    private Vector3 rotateStartPosition;
    private Vector3 rotateCurrentPosition;

    private float currentZoomMagnitude;

    private void Start()
    {
        instance = this;

        newPosition = transform.position;
        newRotation = transform.rotation;
        newZoom = cameraTransform.localPosition;
        currentZoomMagnitude = newZoom.magnitude;
    }

    void LateUpdate()
    {
        // Check if the mouse is over a UI element
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            // Skip camera handling if the pointer is over UI
            return;
        }

        if (followTransform != null)
        {
            HandleTrackingMode();
        }
        else
        {
            HandleFreeMode();
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~uiLayerMask))
            {
                if (hit.transform != followTransform)
                {
                    followTransform = null;
                }
            }
        }

        ApplyTransformation();
    }

    void HandleTrackingMode()
    {
        newPosition = followTransform.position;

        HandleZooming();
        HandleRotation();

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (!Physics.Raycast(ray, out hit) || hit.transform != followTransform)
            {
                followTransform = null;
            }
        }
    }

    void HandleFreeMode()
    {
        HandleMouseInput();
        HandleZooming();
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragStartPosition = ray.GetPoint(entry);
            }
        }

        if (Input.GetMouseButton(0))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float entry;

            if (plane.Raycast(ray, out entry))
            {
                dragCurrentPosition = ray.GetPoint(entry);
                Vector3 dragDelta = dragStartPosition - dragCurrentPosition;
                float zoomFactor = Mathf.Log(currentZoomMagnitude, 2) * 0.2f;
                newPosition = transform.position + dragDelta * zoomFactor;
                newPosition = ClampPositionToBoundary(newPosition);
            }
        }

        HandleRotation();
    }

    void HandleZooming()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            float zoomFactor = 1f - Input.mouseScrollDelta.y * 0.1f;
            Vector3 zoomDelta = Vector3.Scale(newZoom, new Vector3(zoomFactor, zoomFactor, zoomFactor)) - newZoom;
            float zoomSpeed = Mathf.Log(currentZoomMagnitude, 2) * 0.2f;
            newZoom += zoomDelta * zoomSpeed;

            currentZoomMagnitude = newZoom.magnitude;
            currentZoomMagnitude = Mathf.Clamp(currentZoomMagnitude, minZoom, maxZoom);
            newZoom = newZoom.normalized * currentZoomMagnitude;
        }
    }

    void HandleRotation()
    {
        if (Input.GetMouseButtonDown(1))
        {
            rotateStartPosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(1))
        {
            rotateCurrentPosition = Input.mousePosition;
            Vector3 difference = rotateStartPosition - rotateCurrentPosition;
            rotateStartPosition = rotateCurrentPosition;

            newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));
        }
    }

    Vector3 ClampPositionToBoundary(Vector3 position)
    {
        float halfWidth = boundaryWidth * 0.5f;
        return new Vector3(
            Mathf.Clamp(position.x, -halfWidth, halfWidth),
            position.y,
            Mathf.Clamp(position.z, -halfWidth, halfWidth)
        );
    }

    void ApplyTransformation()
    {
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.deltaTime * movementTime);
    }
}