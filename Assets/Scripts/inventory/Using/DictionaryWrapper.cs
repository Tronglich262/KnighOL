using System.Collections.Generic;

[System.Serializable]
public class DictionaryWrapper
{
    public List<string> keys = new List<string>();
    public List<string> values = new List<string>();

    public DictionaryWrapper(Dictionary<string, string> dict)
    {
        keys = new List<string>(dict.Keys);
        values = new List<string>(dict.Values);
    }

    public Dictionary<string, string> ToDictionary()
    {
        var result = new Dictionary<string, string>();
        for (int i = 0; i < keys.Count; i++)
        {
            result[keys[i]] = values[i];    
        }
        return result;
    }
}
