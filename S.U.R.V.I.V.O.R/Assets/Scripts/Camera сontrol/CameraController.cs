using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    private CameraControlActions cameraActions;
    private InputAction movement;
    private Transform cameraTransform;

    [SerializeField] private float maxSpeed = 5f;
    private float speed;

    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float damping = 15f;

    [SerializeField] private float zoomStepSize = 2f;
    [SerializeField] private float zoomDampening = 7.5f;
    [SerializeField] private float zoomMinHeight = 5f;
    [SerializeField] private float zoomMaxHeight = 50f;

    private Vector3 targetPosition;

    private float zoomHeight;

    private Vector3 horizontalVelocity;
    private Vector3 lastPosition;

    private void Awake()
    {
        cameraActions = new CameraControlActions();
        cameraTransform = GetComponentInChildren<Camera>().transform;
    }

    private void OnEnable()
    {
        zoomHeight = cameraTransform.localPosition.y;
        cameraTransform.LookAt(transform);

        lastPosition = transform.position;

        movement = cameraActions.Camera.Move;
        cameraActions.Camera.ZoomCamera.performed += ZoomCamera;
        cameraActions.Camera.Enable();
    }

    private void OnDisable()
    {
        cameraActions.Camera.ZoomCamera.performed -= ZoomCamera;
        cameraActions.Camera.Disable();
    }

    private void Update()
    {
        GetKeyboardMovement();

        UpdateVelocity();
        UpdateBasePosition();
        UpdateCameraPosition();
    }

    private void GetKeyboardMovement()
    {
        var receivedValue = movement.ReadValue<Vector2>();
        var inputValue = receivedValue.x * GetCameraRight()
                         + receivedValue.y * GetCameraUp();
        
        inputValue = inputValue.normalized;
        
        if (inputValue.sqrMagnitude > 0.1f)
            targetPosition += inputValue;
    }

    private void UpdateBasePosition()
    {
        if (targetPosition.sqrMagnitude > 0.1f)
        {
            speed = Mathf.Lerp(speed, maxSpeed, Time.deltaTime * acceleration);
            transform.position += targetPosition * (speed * Time.deltaTime);
        }
        else
        {
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, Time.deltaTime * damping);
            transform.position += horizontalVelocity * Time.deltaTime;
        }

        targetPosition = Vector3.zero;
    }

    private void UpdateVelocity()
    {
        var position = transform.position;
        horizontalVelocity = (position - lastPosition) / Time.deltaTime;
        horizontalVelocity.y = 0f;
        lastPosition = position;
    }

    private Vector3 GetCameraUp()
    {
        var up = cameraTransform.up;
        up.y = 0f;
        return up;
    }

    private Vector3 GetCameraRight()
    {
        var right = cameraTransform.right;
        right.y = 0f;
        return right;
    }

    private void ZoomCamera(InputAction.CallbackContext obj)
    {
        float inputValue = -obj.ReadValue<Vector2>().y / 100f;

        if (Mathf.Abs(inputValue) > 0.1f)
        {
            zoomHeight = cameraTransform.localPosition.y + inputValue * zoomStepSize;

            if (zoomHeight < zoomMinHeight)
                zoomHeight = zoomMinHeight;
            else if (zoomHeight > zoomMaxHeight)
                zoomHeight = zoomMaxHeight;
        }
    }

    private void UpdateCameraPosition()
    {
        var localPosition = cameraTransform.localPosition;
        var zoomTarget = new Vector3(localPosition.x, zoomHeight, localPosition.z);

        localPosition = Vector3.Lerp(localPosition, zoomTarget, Time.deltaTime * zoomDampening);
        cameraTransform.localPosition = localPosition;
    }
}