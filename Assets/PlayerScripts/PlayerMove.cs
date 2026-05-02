using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : NetworkBehaviour
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

    [Networked] 
    private float currentSpeed { get; set; }

    private Rigidbody _rb;
    private InputAction _steer;
    private InputAction _gas;
    private InputAction _brake;
    private bool _ownsInputLifecycle;

    public override void Spawned()
    {
        // init rigidbody on spawn
        _rb = GetComponent<Rigidbody>();
        
        // camera follow only player
        if (HasInputAuthority)
        {
            // TODO:
        }
    }

    private void Awake()
    {
        bool hasAllRefs = steerAction && throttleAction && brakeAction;
        bool hasAnyRef = steerAction || throttleAction || brakeAction;
        if (hasAnyRef && !hasAllRefs)
        {
            Debug.LogError("PlayerMove: assign all three InputActionReferences or leave all empty.", this);
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
            Debug.LogWarning("PlayerMove: No Input source found.", this);
            enabled = false;
            return;
        }

        InputActionMap map = playerInput.actions.FindActionMap("Driving", throwIfNotFound: false);
        if (map != null)
        {
            _steer = map.FindAction("Steer", throwIfNotFound: false);
            _gas = map.FindAction("Gas", throwIfNotFound: false);
            _brake = map.FindAction("Brake", throwIfNotFound: false);
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

    // fusion fun
    public override void FixedUpdateNetwork()
    {
        //print("FixedUpdateNetwork: " + Runner.DeltaTime);

        // input auth check, only they can move
        if (!HasInputAuthority) return;
        if (_steer == null || _gas == null || _brake == null) return;
        
        // 1. read input
        float steerInput = _steer.ReadValue<float>();
        float gasInput = ReadPedal01(_gas);
        float brakeInput = ReadPedal01(_brake);

        // TODO: delete debug once speed works properly
        Debug.Log($"Gas: {gasInput} | Speed: {currentSpeed}");

        float combinedInput = gasInput - brakeInput * (brakePower / 16f);

        // 2. calc speed
        currentSpeed += combinedInput * acceleration * Runner.DeltaTime;
        currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, drag * Runner.DeltaTime);
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed * 0.5f, maxSpeed);

        Debug.Log($"Combined Input: {combinedInput} | Current Speed: {currentSpeed}");

        // 1. calculate turn
        float turnAmount = steerInput * turnSpeed * Mathf.Clamp01(Mathf.Abs(currentSpeed));
        _rb.angularVelocity = new Vector3(0, turnAmount, 0);

        // 2. apply Velocity instead of MovePosition
        _rb.linearVelocity = transform.forward * currentSpeed;
    }

    private static float ReadPedal01(InputAction action)
    {
        float raw = action.ReadValue<float>();
        string path = action.activeControl?.path ?? string.Empty;
        if (path.IndexOf("trigger", StringComparison.OrdinalIgnoreCase) >= 0)
            return Mathf.Clamp01(raw);

        return NormalizeWheelPedal(raw);
    }

    private static float NormalizeWheelPedal(float raw)
    {
        float v = (raw + 1f) * 0.5f;
        return 1f - Mathf.Clamp01(v);
    }
}