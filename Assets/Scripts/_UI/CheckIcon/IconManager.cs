using System.Collections.Generic;
using UnityEngine;

public class IconManager : MonoBehaviour
{
    public static IconManager Instance { get; private set; }

    private readonly Dictionary<string, Sprite> iconDict = new();

    [SerializeField] private string iconFolder = "Icons";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadAllIcons();
    }

    private void LoadAllIcons()
    {
        iconDict.Clear();

        Sprite[] allSprites = Resources.LoadAll<Sprite>(iconFolder);
        foreach (Sprite sprite in allSprites)
        {
            if (sprite == null) continue;

            if (!iconDict.ContainsKey(sprite.name))
                iconDict.Add(sprite.name, sprite);
        }

        Debug.Log($"[IconManager] Loaded {iconDict.Count} icons from Resources/{iconFolder}");
    }

    public Sprite LoadSpriteFromTexture(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return null;

        if (iconDict.TryGetValue(fullName, out var found))
            return found;

        string noColor = fullName.Split('#')[0];
        if (iconDict.TryGetValue(noColor, out found))
            return found;

        string[] parts = noColor.Split('.');
        string lastPart = parts.Length > 0 ? parts[^1] : noColor;
        if (iconDict.TryGetValue(lastPart, out found))
            return found;

        int bracketIndex = lastPart.IndexOf('[');
        if (bracketIndex >= 0)
        {
            string clean = lastPart.Substring(0, bracketIndex).Trim();
            if (iconDict.TryGetValue(clean, out found))
                return found;
        }

        return null;
    }
}