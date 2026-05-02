using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class Spawner : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkPrefabRef playerPrefab;
    private NetworkRunner _runner;
    public Transform spawnPointA;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    private void OnGUI()
    {
        if (_runner == null)
        {
            if (GUI.Button(new Rect(10, 10, 200, 40), "Host Session"))
            {
                StartGame(GameMode.Shared);
            }
            if (GUI.Button(new Rect(10, 60, 200, 40), "Join Session"))
            {
                StartGame(GameMode.Shared);
            }
        }
    }

    async void StartGame(GameMode mode)
    {
        // 1. create runner comp
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;

        // 2. create scene manager if doesnt exist
        var sceneManager = gameObject.GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null) sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();

        // 3. session start
        await _runner.StartGame(new StartGameArgs
        {
            GameMode = mode,
            SessionName = "CarRaceRoom",
            SceneManager = sceneManager
        });
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // master client controls car spawn amount
        if (runner.IsSharedModeMasterClient)
        {
            Debug.Log("Spawning car for player: " + player);

            // TODO: make a spawn point in scene, change to var
            Vector3 pos = spawnPointA != null ? spawnPointA.position : new Vector3(8, 2, -10);
            // TODO: fix player offset, change to spawn second player at spawnPointB or just calc the offset
            //pos.x += player.RawEncoded * 3;
            runner.Spawn(playerPrefab, pos, spawnPointA.rotation, player);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (_spawnedCharacters.TryGetValue(player, out NetworkObject networkObject))
        {
            if (networkObject != null) runner.Despawn(networkObject);
            _spawnedCharacters.Remove(player);
        }
    }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
}