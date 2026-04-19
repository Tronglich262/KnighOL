using System.Collections.Generic;
using UnityEngine;

public class UIItemPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private Transform parent;
    [SerializeField] private int preloadCount = 10;

    private readonly Queue<GameObject> pool = new();
    private readonly List<GameObject> activeItems = new();

    private void Awake()
    {
        for (int i = 0; i < preloadCount; i++)
        {
            GameObject go = CreateNew();
            ReturnToPool(go);
        }
    }

    private GameObject CreateNew()
    {
        GameObject go = Instantiate(prefab, parent);
        go.SetActive(false);
        return go;
    }

    public GameObject Get()
    {
        GameObject go = pool.Count > 0 ? pool.Dequeue() : CreateNew();
        go.SetActive(true);
        activeItems.Add(go);
        return go;
    }

    public void ReleaseAll()
    {
        for (int i = activeItems.Count - 1; i >= 0; i--)
        {
            ReturnToPool(activeItems[i]);
        }
        activeItems.Clear();
    }

    private void ReturnToPool(GameObject go)
    {
        if (go == null) return;
        go.SetActive(false);
        go.transform.SetParent(parent, false);
        pool.Enqueue(go);
    }
}