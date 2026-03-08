using UnityEngine;
using UnityEngine.InputSystem;

public class CameraOrbitController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform cameraTransform;

    [Header("Follow")]
    [SerializeField] private Vector3 pivotOffset = new Vector3(0f, 1.6f, 0f);
    [SerializeField] private float distance = 4f;
    [SerializeField] private float minDistance = 0.75f;
    [SerializeField] private float positionSmoothTime = 0.05f;

    [Header("Rotation")]
    [SerializeField] private float mouseSensitivity = 0.15f;
    [SerializeField] private float stickSensitivity = 120f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 70f;

    [Header("Collision")]
    [SerializeField] private float collisionRadius = 0.2f;
    [SerializeField] private float collisionOffset = 0.1f;
    [SerializeField] private LayerMask collisionLayers = ~0;

    private Vector2 lookInput;
    private Vector3 cameraVelocity;
    private float yaw;
    private float pitch = 15f;

    private void Awake()
    {
        if (target == null)
        {
            target = transform;
        }

        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraTransform != null)
        {
            Vector3 euler = cameraTransform.eulerAngles;
            yaw = euler.y;
            pitch = NormalizeAngle(euler.x);
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
    }

    private void LateUpdate()
    {
        if (target == null || cameraTransform == null)
        {
            return;
        }

        UpdateRotation();
        UpdateCameraPosition();
    }

    private void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    private void UpdateRotation()
    {
        bool usingMouse = Mouse.current != null && Mouse.current.delta.IsActuated();
        float sensitivity = usingMouse ? mouseSensitivity : stickSensitivity * Time.deltaTime;

        yaw += lookInput.x * sensitivity;
        pitch -= lookInput.y * sensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    private void UpdateCameraPosition()
    {
        Vector3 pivotPosition = target.position + pivotOffset;
        Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredDirection = orbitRotation * Vector3.back;
        float targetDistance = ResolveCameraDistance(pivotPosition, desiredDirection);
        Vector3 desiredPosition = pivotPosition + desiredDirection * targetDistance;

        cameraTransform.position = Vector3.SmoothDamp(
            cameraTransform.position,
            desiredPosition,
            ref cameraVelocity,
            positionSmoothTime
        );

        cameraTransform.rotation = Quaternion.LookRotation(pivotPosition - cameraTransform.position);
    }

    private float ResolveCameraDistance(Vector3 pivotPosition, Vector3 direction)
    {
        if (Physics.SphereCast(pivotPosition, collisionRadius, direction, out RaycastHit hit, distance, collisionLayers, QueryTriggerInteraction.Ignore))
        {
            float blockedDistance = hit.distance - collisionOffset;
            return Mathf.Clamp(blockedDistance, minDistance, distance);
        }

        return distance;
    }

    private float NormalizeAngle(float angle)
    {
        if (angle > 180f)
        {
            angle -= 360f;
        }

        return angle;
    }
}
