using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Threading.Tasks;

public class FusionManager : MonoBehaviour
{
    public static FusionManager Instance;

    public NetworkRunner NetworkRunnerPrefab;
    private NetworkRunner runner;

    public PlayerSpawner playerSpawner; 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // nếu muốn giữ qua scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async void StartFusionSession(string sceneName)
    {
        // Kiểm tra scene đã có trong Build Settings
#if UNITY_EDITOR
        if (!UnityEditor.EditorBuildSettings.scenes.Any(i => i.path.Contains(sceneName) && i.enabled))
        {
            UnityEditor.EditorUtility.DisplayDialog("Hero Editor", $"Please add '{sceneName}.scene' to Build Settings!", "OK");
            return;
        }
#endif
        // Tạo NetworkRunner
        if (runner != null)
        {
            Destroy(runner.gameObject);
            runner = null;
        }

        GameObject runnerGO = Instantiate(NetworkRunnerPrefab.gameObject);
        runner = runnerGO.GetComponent<NetworkRunner>();
        runner.ProvideInput = true;

        // Tìm PlayerSpawner trong scene mới
        if (playerSpawner == null)
        {
            playerSpawner = FindFirstObjectByType<PlayerSpawner>();
            if (playerSpawner == null)
            {
                Debug.LogError("Không tìm thấy PlayerSpawner trong scene!");
                return;
            }
        }

        runner.AddCallbacks(playerSpawner);

        int sceneIndex = GetBuildIndexFromSceneName(sceneName);
        if (sceneIndex == -1) return;

        await runner.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = "MainSession",
            Scene = SceneRef.FromIndex(sceneIndex),
            SceneManager = runnerGO.GetComponent<NetworkSceneManagerDefault>()
        });
    }

    private int GetBuildIndexFromSceneName(string sceneName)
    {
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(i);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == sceneName)
                return i;
        }
        Debug.LogError($"Scene '{sceneName}' not found in Build Settings!");
        return -1;
    }
}
