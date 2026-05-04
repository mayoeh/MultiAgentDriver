using Fusion;
using UnityEngine;

public class FusionStart : MonoBehaviour
{
    private static NetworkObject localPlayerCar;

    void Awake()
    {
        localPlayerCar = GameObject.Find("CarRoot").GetComponent<NetworkObject>();
    }

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