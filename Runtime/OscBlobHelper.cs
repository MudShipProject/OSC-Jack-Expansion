using UnityEngine;

namespace OscJackExpansion
{
    public static class OscBlobHelper
    {
        public static byte[] Serialize<T>(T obj)
        {
            string json = JsonUtility.ToJson(obj);
            return System.Text.Encoding.UTF8.GetBytes(json);
        }

        public static T Deserialize<T>(byte[] data)
        {
            string json = System.Text.Encoding.UTF8.GetString(data);
            return JsonUtility.FromJson<T>(json);
        }
    }
}
