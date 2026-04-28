using Fusion;
using UnityEngine;

public class FusionStart : MonoBehaviour
{
    public NetworkObject localPlayerCar;

    async void Start()
    {
        var runner = GetComponent<NetworkRunner>();

        var result = await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = "RacingRoom"
        });

        if (result.Ok)
        {
            localPlayerCar.RequestStateAuthority();
        }
    }
}