using System;
using UnityEngine;

namespace HealthFiles
{
    /// <summary>
    /// WP3 Helper: Unity's JsonUtility cannot directly parse top-level JSON arrays like [ {...}, {...} ].
    /// Because WP1's Hugging Face pipeline exports raw JSON arrays (e.g. alex_lines.json),
    /// this tiny wrapper lets Unity read WP1's files with zero manual editing.
    /// </summary>
    public static class JsonArrayHelper
    {
        [Serializable]
        private class Wrapper<T>
        {
            public T[] items;
        }

        public static T[] FromJson<T>(string rawJsonArray)
        {
            if (string.IsNullOrWhiteSpace(rawJsonArray))
                return Array.Empty<T>();

            string trimmed = rawJsonArray.Trim();
            string wrapped = "{\"items\":" + trimmed + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrapped);
            return wrapper != null && wrapper.items != null ? wrapper.items : Array.Empty<T>();
        }
    }
}
