using UnityEngine;

public static class JsonArrayHelper
{
    public static T[] FromJson<T>(string json)
    {
        string wrappedJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
        return wrapper.array;
    }

    [System.Serializable]
    private class Wrapper<T> { public T[] array; }
}

