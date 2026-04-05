using UnityEngine;

public static class JsonPacketSerializer
{
    public static string Serialize<T>(T data)
    {
        return JsonUtility.ToJson(data);
    }

    public static T Deserialize<T>(string json)
    {
        return JsonUtility.FromJson<T>(json);
    }
}