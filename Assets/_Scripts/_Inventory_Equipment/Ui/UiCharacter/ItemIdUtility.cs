using System;

public static class ItemIdUtility
{
    public static string Normalize(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        id = id.Trim();

        int hashIndex = id.IndexOf('#');
        if (hashIndex >= 0)
            id = id.Substring(0, hashIndex).Trim();

        id = id.Replace("\n", "")
               .Replace("\r", "")
               .Replace("\t", "");

        return id.Trim();
    }
}