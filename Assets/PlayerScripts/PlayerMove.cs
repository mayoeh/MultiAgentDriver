using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference steerAction;
    public InputActionReference throttleAction;
    public InputActionReference brakeAction;

    [Header("Movement")]
    public float acceleration = 12f;
    public float brakePower = 16f;
    public float turnSpeed = 90f;
    public float maxSpeed = 15f;
    public float drag = 2f;

    private float currentSpeed = 0f;

    private void OnEnable()
    {
        steerAction.action.Enable();
        throttleAction.action.Enable();
        brakeAction.action.Enable();
    }

    private void OnDisable()
    {
        steerAction.action.Disable();
        throttleAction.action.Disable();
        brakeAction.action.Disable();
    }

    private void Update()
    {
        float steer = steerAction.action.ReadValue<float>();
        float gas = NormalizePedal(throttleAction.action.ReadValue<float>());
        float brake = NormalizePedal(brakeAction.action.ReadValue<float>());

        float input = gas - brake;

        // speed
        currentSpeed += input * acceleration * Time.deltaTime;
        currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, drag * Time.deltaTime);
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed * 0.5f, maxSpeed);

        // rotate based on steering AND movement
        float turn = steer * turnSpeed * Mathf.Clamp01(Mathf.Abs(currentSpeed)) * Time.deltaTime;
        transform.Rotate(0f, turn, 0f);

        // move in facing direction
        transform.position += transform.forward * currentSpeed * Time.deltaTime;
    }

    private float NormalizePedal(float raw)
    {
        float v = (raw + 1f) * 0.5f;
        return 1f - Mathf.Clamp01(v);
    }
}