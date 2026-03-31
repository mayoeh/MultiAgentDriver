using UnityEngine;
using Fusion;

public class NetworkManager : MonoBehaviour
{
    public NetworkRunner runnerPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var runner = Instantiate(runnerPrefab);
        runner.ProvideInput = true;
        runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared,
            SessionName = "MADRoom",
            Scene = SceneRef.FromIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex)
        });
    }
}
