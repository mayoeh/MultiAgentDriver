using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float lookSpeed = 2f;
    public float smoothTime = 0.1f;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isLooking;

    private Vector3 currentVelocity;
    private float rotationX = 0f;

    private CameraControls controls;

    void Awake()
    {
        controls = new CameraControls();

        controls.Camera.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Camera.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Camera.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Camera.Look.canceled += ctx => lookInput = Vector2.zero;

        controls.Camera.EnableLook.performed += _ => isLooking = true;
        controls.Camera.EnableLook.canceled += _ => isLooking = false;
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
    }

    void HandleMovement()
    {
        Vector3 direction = new Vector3(moveInput.x, 0, moveInput.y);
        direction = transform.TransformDirection(direction);

        Vector3 targetVelocity = direction * moveSpeed;
        Vector3 smoothed = Vector3.SmoothDamp(
            GetComponent<Rigidbody>() ? GetComponent<Rigidbody>().linearVelocity : Vector3.zero,
            targetVelocity,
            ref currentVelocity,
            smoothTime
        );

        transform.position += smoothed * Time.deltaTime;
    }

    void HandleMouseLook()
    {
        if (!isLooking) return;

        float mouseX = lookInput.x * lookSpeed;
        float mouseY = lookInput.y * lookSpeed;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);

        transform.localRotation = Quaternion.Euler(rotationX, transform.localEulerAngles.y + mouseX, 0f);
    }
}