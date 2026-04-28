using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    [Tooltip("Optional. Leave all three empty to use the Driving action map from a PlayerInput on a child object.")]
    public InputActionReference steerAction;
    public InputActionReference throttleAction;
    public InputActionReference brakeAction;

    [Header("Movement")]
    public float acceleration = 12f;
    public float brakePower = 16f;
    public float turnSpeed = 90f;
    public float maxSpeed = 15f;
    public float drag = 2f;

    private float currentSpeed;
    private InputAction _steer;
    private InputAction _gas;
    private InputAction _brake;
    private bool _ownsInputLifecycle;

    private void Awake()
    {
        bool hasAllRefs = steerAction && throttleAction && brakeAction;
        bool hasAnyRef = steerAction || throttleAction || brakeAction;
        if (hasAnyRef && !hasAllRefs)
        {
            Debug.LogError("PlayerMove: assign all three InputActionReferences (Steer, Gas, Brake) or leave all empty to use PlayerInput.", this);
            enabled = false;
            return;
        }

        if (hasAllRefs)
        {
            _steer = steerAction.action;
            _gas = throttleAction.action;
            _brake = brakeAction.action;
            _ownsInputLifecycle = true;
            return;
        }

        PlayerInput playerInput = GetComponentInChildren<PlayerInput>();
        if (!playerInput)
        {
            Debug.LogWarning(
                "PlayerMove: no InputActionReferences and no PlayerInput under this object. Add PlayerInput + Wheel Mappings (Driving), or assign actions.",
                this);
            enabled = false;
            return;
        }

        InputActionMap map = playerInput.actions.FindActionMap("Driving", throwIfNotFound: false);
        if (map == null)
        {
            Debug.LogError("PlayerMove: PlayerInput has no 'Driving' action map.", this);
            enabled = false;
            return;
        }

        _steer = map.FindAction("Steer", throwIfNotFound: false);
        _gas = map.FindAction("Gas", throwIfNotFound: false);
        _brake = map.FindAction("Brake", throwIfNotFound: false);
        if (_steer == null || _gas == null || _brake == null)
        {
            Debug.LogError("PlayerMove: Driving map must contain Steer, Gas, and Brake actions.", this);
            enabled = false;
            return;
        }

        _ownsInputLifecycle = false;
    }

    private void OnEnable()
    {
        if (_ownsInputLifecycle)
        {
            _steer?.Enable();
            _gas?.Enable();
            _brake?.Enable();
        }
    }

    private void OnDisable()
    {
        if (_ownsInputLifecycle)
        {
            _steer?.Disable();
            _gas?.Disable();
            _brake?.Disable();
        }
    }

    private void Update()
    {
        if (_steer == null || _gas == null || _brake == null) return;

        float steer = _steer.ReadValue<float>();
        float gas = ReadPedal01(_gas);
        float brake = ReadPedal01(_brake);

        float input = gas - brake * (brakePower / 16f);

        currentSpeed += input * acceleration * Time.deltaTime;
        currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, drag * Time.deltaTime);
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed * 0.5f, maxSpeed);

        float turn = steer * turnSpeed * Mathf.Clamp01(Mathf.Abs(currentSpeed)) * Time.deltaTime;
        transform.Rotate(0f, turn, 0f);

        transform.position += transform.forward * currentSpeed * Time.deltaTime;
    }

    /// <summary>
    /// Wheel axes use -1..1 with an inverted pedal curve. Gamepad triggers are 0..1 and handled directly.
    /// </summary>
    private static float ReadPedal01(InputAction action)
    {
        float raw = action.ReadValue<float>();
        string path = action.activeControl?.path ?? string.Empty;
        if (path.IndexOf("trigger", StringComparison.OrdinalIgnoreCase) >= 0)
            return Mathf.Clamp01(raw);

        return NormalizeWheelPedal(raw);
    }

    /// <summary>
    /// Matches the original arcade curve for Logitech-style -1..1 axes (after Invert on bindings).
    /// </summary>
    private static float NormalizeWheelPedal(float raw)
    {
        float v = (raw + 1f) * 0.5f;
        return 1f - Mathf.Clamp01(v);
    }
}
