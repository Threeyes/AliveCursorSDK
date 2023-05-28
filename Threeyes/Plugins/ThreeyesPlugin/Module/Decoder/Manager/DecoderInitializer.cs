using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace Threeyes.Decoder
{
    /// <summary>
    /// 
    /// Ref: Newtonsoft.Json.UnityConverters.UnityConverterInitializer
    /// </summary>
    public static class DecoderInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#pragma warning disable IDE0051 // Remove unused private members
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        internal static void Init()
#pragma warning restore IDE0051 // Remove unused private members
        {
            //ToAdd:搜集所有的自定义Decoder并设置DecoderManager.listSupportType
            var listDecoderType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => GetLoadableTypes(x))
               .Where(x => typeof(DecoderBase).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
               .OrderBy(type => type.Name);//Search Decoder

            DecoderManager.listDecoder.Clear();
            DecoderManager.listSupportType.Clear();
            foreach (var decoderType in listDecoderType)
            {
                DecoderBase inst = Activator.CreateInstance(decoderType) as DecoderBase;
                if (inst != null)
                {
                    DecoderManager.listDecoder.Add(inst);
                    DecoderManager.listSupportType.Add(inst.SupportType);
                }
            }
        }

        public static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
#if DEBUG
                Console.WriteLine("Failed to load some types from assembly '{assembly.FullName}'. Maybe assembly is not fully loaded yet?\n"
                    + ex.ToString());
#endif
                return ex.Types.Where(t => t != null);
            }
        }
    }
}