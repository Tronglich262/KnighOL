using UnityEngine;
using UnityEngine.SceneManagement;

public class ChekcLoadScene : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("đã va chạm");
            SceneManager.LoadScene("Test2", LoadSceneMode.Single);
        }
    }
}
