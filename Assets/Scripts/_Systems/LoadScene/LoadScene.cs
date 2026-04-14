using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadScene : MonoBehaviour
{

    public void OnclickScene(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
