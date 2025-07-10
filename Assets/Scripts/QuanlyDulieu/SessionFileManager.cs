using System.IO;
using UnityEngine;

public static class SessionFileManager
{
    public static string GetSessionFilePath(string clientId)
    {
        return Path.Combine(Application.persistentDataPath, $"session_{clientId}.json");
    }

    public static void SaveSession(string token, int accountId, string clientId)
    {
        var data = new SessionData { Token = token, AccountId = accountId };
        var json = JsonUtility.ToJson(data);
        File.WriteAllText(GetSessionFilePath(clientId), json);
    }

    public static SessionData LoadSession(string clientId)
    {
        string path = GetSessionFilePath(clientId);
        if (!File.Exists(path)) return null;
        var json = File.ReadAllText(path);
        return JsonUtility.FromJson<SessionData>(json);
    }

    public static void ClearSession(string clientId)
    {
        string path = GetSessionFilePath(clientId);
        if (File.Exists(path)) File.Delete(path);
    }
}

[System.Serializable]
public class SessionData
{
    public string Token;
    public int AccountId;
}
