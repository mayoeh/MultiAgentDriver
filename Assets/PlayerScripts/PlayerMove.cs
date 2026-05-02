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
    [Tooltip("Optional. Leave all three empty to use the Driving action map from a PlayerInput on a child object.")]
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
        
        // register local instance
        if (HasInputAuthority)
        {
            Runner.AddCallbacks(this);
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        // clean callbacks
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

        // 1. calc network speed
        currentSpeed += combinedInput * acceleration * Runner.DeltaTime;
        currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, drag * Runner.DeltaTime);
        currentSpeed = Mathf.Clamp(currentSpeed, -maxSpeed * 0.5f, maxSpeed);

        // 2. calc turning
        float turnAmount = data.steer * turnSpeed * Mathf.Clamp01(Mathf.Abs(currentSpeed));
        _rb.angularVelocity = new Vector3(0, turnAmount, 0);

        // 3. apply velocity
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

    public void BeforeUpdate()
    {
        throw new NotImplementedException();
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        throw new NotImplementedException();
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        throw new NotImplementedException();
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        throw new NotImplementedException();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        throw new NotImplementedException();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
        throw new NotImplementedException();
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
        throw new NotImplementedException();
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        throw new NotImplementedException();
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        throw new NotImplementedException();
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        throw new NotImplementedException();
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        throw new NotImplementedException();
    }
}