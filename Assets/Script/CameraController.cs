using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Current;

    public float moveSensitivity = 2;
    public float zoomSensitivity = 3;
    public float zoomSpeed = 5;
    public float moveSpeed = 0.1f;
    public float minFOV = 5;
    public float maxFOV = 60;
    public bool isMoving;
    public bool isZooming;
    public bool canTouch = true;

    private Camera cameraMain;
    private Vector3 lastMousePosition;
    private float lastFieldOfView;
    private Vector3 lastPosition;

    private void Awake()
    {
        Current = this;

        cameraMain = Camera.main;
        lastFieldOfView = cameraMain.fieldOfView;
        lastPosition = cameraMain.transform.position;
    }

    private void Update()
    {
        if (!canTouch) return;

        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

            float difference = (currentMagnitude - prevMagnitude) * zoomSensitivity * 0.01f;

            lastFieldOfView = Mathf.Clamp(lastFieldOfView - difference, minFOV, maxFOV);

            isZooming = true;
        }
        else if (isZooming)
        {
            if (Input.touchCount == 0)
            {
                isZooming = false;
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                lastMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0))
            {
                var delta = Input.mousePosition - lastMousePosition;
                if (delta.magnitude > 2)
                {
                    lastPosition -= 0.001f * moveSensitivity * new Vector3(delta.x, 0, delta.y);
                    lastMousePosition = Input.mousePosition;
                    isMoving = true;
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isMoving = false;
            }
        }

        lastFieldOfView = Mathf.Clamp(lastFieldOfView - Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity * 10, minFOV, maxFOV);
    }

    private Vector3 cameraVelocity;
    private void LateUpdate()
    {
        cameraMain.transform.position = Vector3.SmoothDamp(cameraMain.transform.position, lastPosition, ref cameraVelocity, moveSpeed, Mathf.Infinity, Time.deltaTime);
        cameraMain.fieldOfView = Mathf.Lerp(cameraMain.fieldOfView, lastFieldOfView, Time.deltaTime * zoomSpeed);
    }
}
