using System;
using UnityEngine;
#if USE_JsonDotNet
using Newtonsoft.Json;
#endif
namespace Threeyes.Persistent
{
    public class PersistentConverter_Json : IPersistentConverter
    {
#if USE_JsonDotNet

        //1.None is short
        //2.Formatting.Indented is good for reading
        public Formatting formatting = Formatting.Indented;

        public PersistentConverter_Json(Formatting formatting)
        {
            this.formatting = formatting;
        }
#endif

        public PersistentConverter_Json()
        {
        }

        public string Serialize<T>(T obj)
        {
#if USE_JsonDotNet
            return JsonConvert.SerializeObject(obj, formatting);
#else
            Debug.LogError("Please Active USE_JsonDotNet!");
            return null;
#endif

        }
        public T Deserialize<T>(string content)
        {
#if USE_JsonDotNet
            if (content.IsNullOrEmpty())
                return default(T);
            return JsonConvert.DeserializeObject<T>(content);
#else
            Debug.LogError("Please Active USE_JsonDotNet!");
            return default(T);
#endif
        }
        public T Deserialize<T>(string content, Type realType)
        {
#if USE_JsonDotNet
            if (content.IsNullOrEmpty() || realType == null)
                return default(T);
            return (T)JsonConvert.DeserializeObject(content, realType);
#else
            Debug.LogError("Please Active USE_JsonDotNet!");
            return default(T);
#endif
        }
    }
}