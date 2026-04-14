/*using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRunnerHandler : MonoBehaviour
{
    public NetworkRunner networkRunnerPrefab;
    //liên kết Fusion với hệ thống Unity
    private async void Start()
    {
        var runner = Instantiate(networkRunnerPrefab);
        runner.name = "NetworkRunner";

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Shared, // hoặc Host, Client
            SessionName = "Login",
            Scene = SceneRef.FromIndex(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex),
            SceneManager = runner.GetComponent<NetworkSceneManagerDefault>()
        });

    }
}*/