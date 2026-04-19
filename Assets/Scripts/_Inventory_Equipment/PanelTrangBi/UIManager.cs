using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject itemDetailsCanvasPrefab;

    void Awake()
    {
        if (ItemDetailsPanel.Instance == null)
        {
            GameObject canvas = Instantiate(itemDetailsCanvasPrefab);
            DontDestroyOnLoad(canvas); // hoặc để nó nằm trong CharacterCanvas
        }
    }
}
