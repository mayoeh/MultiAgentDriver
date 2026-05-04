using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Fusion;
using Fusion.Sockets;
using System.Collections.Generic;

public struct NetworkInputData : INetworkInput
{
    public float steer;
    public float gas;
    public float brake;
}

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : NetworkBehaviour, IBeforeUpdate, INetworkRunnerCallbacks
{
    [Header("Input References")]
    public InputActionReference steerAction;
    public InputActionReference throttleAction;
    public InputActionReference brakeAction;

    [Header("Movement Settings")]
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
        _rb = GetComponent<Rigidbody>();

        // setup cameras
        Camera internalCam = GetComponentInChildren<Camera>(true);
        CarCameraRig rigScript = GetComponentInChildren<CarCameraRig>(true); 

        if (HasInputAuthority)
        {
            // register for input updates
            Runner.AddCallbacks(this);

            if (internalCam != null) 
            {
                internalCam.gameObject.SetActive(true);
                internalCam.enabled = true;
                internalCam.tag = "MainCamera";
            }
            if (rigScript != null) rigScript.enabled = true;
        }
        else
        {
            if (internalCam != null) 
            {
                internalCam.tag = "Untagged";
                internalCam.enabled = false;
                internalCam.gameObject.SetActive(false);
            }
            if (rigScript != null) rigScript.enabled = false;
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        runner.RemoveCallbacks(this);
    }

    private void Awake()
    {
        bool hasAllRefs = steerAction && throttleAction && brakeAction;
        if (hasAllRefs)
        {
            _steer = steerAction.action;
            _gas = throttleAction.action;
            _brake = brakeAction.action;
            _ownsInputLifecycle = true;
        }
        else
        {
            PlayerInput playerInput = GetComponentInChildren<PlayerInput>();
            if (playerInput)
            {
                InputActionMap map = playerInput.actions.FindActionMap("Driving", false);
                if (map != null)
                {
                    _steer = map.FindAction("Steer", false);
                    _gas = map.FindAction("Gas", false);
                    _brake = map.FindAction("Brake", false);
                }
            }
            _ownsInputLifecycle = false;
        }
    }

    private void OnEnable() => ToggleActions(true);
    private void OnDisable() => ToggleActions(false);

    private void ToggleActions(bool enable)
    {
        if (!_ownsInputLifecycle) return;
        if (enable) { _steer?.Enable(); _gas?.Enable(); _brake?.Enable(); }
        else { _steer?.Disable(); _gas?.Disable(); _brake?.Disable(); }
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();

        if (_steer != null) data.steer = _steer.ReadValue<float>();
        if (_gas != null) data.gas = ReadPedal01(_gas);
        if (_brake != null) data.brake = ReadPedal01(_brake);

        input.Set(data);
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            ApplyMovement(data);
        }
    }

    private void ApplyMovement(NetworkInputData data)
    {
        float combinedInput = data.gas - (data.brake * (brakePower / 16f));

        currentSpeed += combinedInput * acceleration * Runner.DeltaTime;
        currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, drag * Runner.DeltaTime);
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed * 0.5f, maxSpeed);

        float turnAmount = data.steer * turnSpeed * Mathf.Clamp01(Mathf.Abs(currentSpeed));
        _rb.angularVelocity = new Vector3(0, turnAmount, 0);
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
    

    public void BeforeUpdate() { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
}