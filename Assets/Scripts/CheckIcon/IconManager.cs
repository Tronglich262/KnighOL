using System.Collections.Generic;
using UnityEngine;

public class IconManager : MonoBehaviour
{
    public static IconManager Instance;

    private Dictionary<string, Sprite> iconDict = new Dictionary<string, Sprite>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadAllIcons(); // Tự load toàn bộ icon 1 lần
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadAllIcons()
    {
        Sprite[] allSprites = Resources.LoadAll<Sprite>(""); // Duyệt tất cả Resources
        foreach (Sprite sprite in allSprites)
        {
            if (!iconDict.ContainsKey(sprite.name))
            {
                iconDict.Add(sprite.name, sprite);
            }
        }

        Debug.Log(" Loaded " + iconDict.Count + " icon từ Resources.");
    }

    public Sprite LoadSpriteFromTexture(string fullName)
    {
        // Tự động thử:
        // 1. fullName gốc
        // 2. fullName sau khi bỏ màu #...
        // 3. Tên cuối
        // 4. Tên sau khi bỏ tag [Paint]

        List<string> tryNames = new List<string>();

        tryNames.Add(fullName); // Gốc

        // Bỏ mã màu
        string noColor = fullName.Split('#')[0];
        tryNames.Add(noColor);

        // Lấy tên cuối
        string[] parts = noColor.Split('.');
        string lastPart = parts.Length > 0 ? parts[^1] : noColor;
        tryNames.Add(lastPart);

        // Bỏ tag [Paint] nếu có
        if (lastPart.Contains("["))
        {
            string clean = lastPart.Substring(0, lastPart.IndexOf('[')).Trim();
            tryNames.Add(clean);
        }

        // Thử lần lượt từng cách
        foreach (var name in tryNames)
        {
            if (iconDict.TryGetValue(name, out Sprite found))
                return found;
        }

        return null;
    }
}
