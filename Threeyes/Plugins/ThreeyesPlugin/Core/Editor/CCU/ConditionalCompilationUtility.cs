#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Compilation;
using Assembly = System.Reflection.Assembly;
using Debug = UnityEngine.Debug;
using System.Threading;

/// <summary>
/// Ref: https://github.com/Unity-Technologies/conditionalcompilationutility
/// 
/// 功能：
/// -根据给定类名，自动添加宏定义。当前库是使用[OptionalDependencyAttribute]作为标识
/// 
/// PS:
/// -只有代码无报错，以下方法才会执行！
/// 
/// ToUpdate:
/// -提供一个额外的窗口，方便用户自行选择并激活已经识别到的插件。也可以是用户手动点击扫描更新宏定义，避免每次加载都需要检测导致循环报错。（新增：是否自动刷新，还有一个主动刷新按钮）
/// </summary>
//namespace ConditionalCompilation
namespace Threeyes.Core.Editor//更改命名空间，避免其他库含有该类导致冲突
{
    /// <summary>
    /// The Conditional Compilation Utility (CCU) will add defines to the build settings once dependendent classes have been detected.
    /// A goal of the CCU was to not require the CCU itself for other libraries to specify optional dependencies. So, it relies on the
    /// specification of at least one custom attribute in a project that makes use of it. Here is an example:
    ///
    /// [Conditional(UNITY_CCU)]                                    // | This is necessary for CCU to pick up the right attributes
    /// public class OptionalDependencyAttribute : Attribute        // | Must derive from System.Attribute
    /// {
    ///     public string dependentClass;                           // | Required field specifying the fully qualified dependent class
    ///     public string define;                                   // | Required field specifying the define to add
    /// }
    ///
    /// Then, simply specify the assembly attribute(s) you created in any of your C# files:
    /// [assembly: OptionalDependency("UnityEngine.InputNew.InputSystem", "USE_NEW_INPUT")]
    /// [assembly: OptionalDependency("Valve.VR.IVRSystem", "ENABLE_STEAMVR_INPUT")]
    ///
    /// namespace Foo
    /// {
    /// ...
    /// }
    /// </summary>
    [InitializeOnLoad]//Warning：Asset operations such as asset loading should be avoided in InitializeOnLoad methods. InitializeOnLoad methods are called before asset importing is completed and therefore the asset loading can fail resulting in a null object. To do initialization after a domain reload which requires asset operations use the AssetPostprocessor.OnPostprocessAllAssets callback. This callback supports all asset operations and has a parameter signaling if there was a domain reload.（https://docs.unity3d.com/ScriptReference/InitializeOnLoadAttribute.html）
    static class ConditionalCompilationUtility
    {
        const string prefs_AutoUpdate = "CCU_AutoUpdate";

        const string k_PreviousUnsuccessfulDefines = "ConditionalCompilationUtility.PreviousUnsuccessfulDefines";
        const string k_EnableCCU = "UNITY_CCU";

        public static bool AutoUpdate
        {
            get
            {
                bool canAutoUpdate = EditorPrefs.GetBool(prefs_AutoUpdate, true);//默认可以自动更新
                return canAutoUpdate;
                //var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                //return PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup).Contains(k_EnableCCU);
            }
            set
            {
                EditorPrefs.SetBool(prefs_AutoUpdate, value);
            }
        }

        static ConditionalCompilationUtility()
        {
#if UNITY_2017_3_OR_NEWER
            //var errorsFound = false;

            //# 如果出错，则恢复成上次成功的Defines，避免陷入死循环（暂时注释，因为可能会因为其他代码临时编译出错导致意外删掉宏定义，后续可提供手动更新的UI)
            //CompilationPipeline.assemblyCompilationFinished += (outputPath, compilerMessages) =>
            //{
            //    var errorCount = compilerMessages.Count(m => m.type == CompilerMessageType.Error && m.message.Contains("CS0246"));
            //    if (errorCount > 0 && !errorsFound)
            //    {
            //        var previousDefines = EditorPrefs.GetString(k_PreviousUnsuccessfulDefines);
            //        var currentDefines = string.Join(";", defines);
            //        if (currentDefines != previousDefines)
            //        {
            //            // Store the last set of unsuccessful defines to avoid ping-ponging
            //            EditorPrefs.SetString(k_PreviousUnsuccessfulDefines, currentDefines);

            //            // Since there were errors in compilation, try removing any dependency defines
            //            UpdateDependencies(true);
            //        }
            //        errorsFound = true;
            //    }
            //};

            if (!AutoUpdate)
                return;

            AssemblyReloadEvents.afterAssemblyReload += () =>
            {
                //if (!errorsFound)
                UpdateDependencies();
            };
#else
            UpdateDependencies();
#endif
        }

        /// <summary>
        /// 获取项目中所有的[OptionalDependency]相关信息
        /// </summary>
        /// <param name="projectDefines">当前的项目宏定义</param>
        /// <returns></returns>
        public static List<OptionalDependencyInfo> GetODIs(out List<string> projectDefines)
        {
            List<OptionalDependencyInfo> listInfo_All = new List<OptionalDependencyInfo>();
            string previousProjectDefines = PlayerSettings.GetScriptingDefineSymbolsForGroup(ActiveBuildTargetGroup);
            projectDefines = previousProjectDefines.Split(';').ToList();//项目当前的宏定义
            List<string> projectDefinesClone = projectDefines.SimpleClone();

            //——扫描并添加自定义宏定义——
            //#1 查找所有使用[OptionalDependency]相似字段的Attribute类型
            var conditionalAttributeType = typeof(ConditionalAttribute);
            const string kDependentClass = "dependentClass";
            const string kDefine = "define";
            const string kUsedBy = "usedBy";
            var attributeTypes = GetAssignableTypes(typeof(Attribute), type =>
            {
                var conditionals = (ConditionalAttribute[])type.GetCustomAttributes(conditionalAttributeType, true);

                foreach (var conditional in conditionals)
                {
                    if (string.Equals(conditional.ConditionString, k_EnableCCU, StringComparison.OrdinalIgnoreCase))
                    {
                        var dependentClassField = type.GetField(kDependentClass);
                        if (dependentClassField == null)
                        {
                            Debug.LogErrorFormat("[CCU] Attribute type {0} missing field: {1}", type.Name, kDependentClass);
                            return false;
                        }

                        var defineField = type.GetField(kDefine);
                        if (defineField == null)
                        {
                            Debug.LogErrorFormat("[CCU] Attribute type {0} missing field: {1}", type.Name, kDefine);
                            return false;
                        }

                        return true;
                    }
                }
                return false;
            });

            //#2 获取上述所有Attributes实例的相关信息
            ForEachAssembly(assembly =>
            {
                //通过反射扫描并获取有类似字段的Attribite
                var typeAttributes = assembly.GetCustomAttributes(false).Cast<Attribute>();
                foreach (var typeAttribute in typeAttributes)
                {
                    if (attributeTypes.Contains(typeAttribute.GetType()))
                    {
                        var t = typeAttribute.GetType();

                        // These fields were already validated in a previous step
                        var dependentClass = t.GetField(kDependentClass).GetValue(typeAttribute) as string;
                        var define = t.GetField(kDefine).GetValue(typeAttribute) as string;
                        var usedBy = t.GetField(kUsedBy).GetValue(typeAttribute) as string;//可以为空

                        if (!string.IsNullOrEmpty(dependentClass) && !string.IsNullOrEmpty(define)/* && !listInfo_All.Exists(info => info.dependentClass == dependentClass)*/)
                        {
                            //查找或创建对应Info
                            OptionalDependencyInfo info = listInfo_All.FirstOrDefault(i => i.define == define);//尝试查找已有的Info（因为可能多个库定义并引用了相同的库，仅检查起决定作用的Define）
                            bool elementNotExist = info == null;
                            if (elementNotExist)
                                info = new OptionalDependencyInfo(dependentClass, define);

                            //添加UseBy
                            if (!string.IsNullOrEmpty(usedBy))
                                info.AddUseBy(usedBy);

                            if (elementNotExist)
                                listInfo_All.Add(info);
                            //Debug.LogError("[Test} " + dependentClass + "_" + usedBy);
                        }
                    }
                }
            });

            //#3 记录每个ODI的状态
            ForEachAssembly(assembly =>
            {
                foreach (var info in listInfo_All)
                {
                    var typeName = info.dependentClass;
                    var define = info.define;

                    var type = assembly.GetType(typeName);
                    if (type != null)//项目包含该类
                    {
                        info.inProject = true;
                        if (projectDefinesClone.Contains(define, StringComparer.OrdinalIgnoreCase))//该宏定义已添加
                        {
                            info.isActive = true;
                        }
                    }
                }
            });

            return listInfo_All;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reset">ture: Remove all custom defines</param>
        public static void UpdateDependencies(bool reset = false)
        {
            List<string> projectDefines;
            List<OptionalDependencyInfo> listInfo_All = GetODIs(out projectDefines);
            List<string> oldProjectDefines = projectDefines.SimpleClone();//保存旧的配置，用于比较


            //——更新项目宏定义——
            //#1 如果没有UNITY_CCU宏定义则自动添加，等待项目刷新并返回
            if (!projectDefines.Contains(k_EnableCCU, StringComparer.OrdinalIgnoreCase))
            {
                EditorApplication.LockReloadAssemblies();
                projectDefines.Add(k_EnableCCU);
                // This will trigger another re-compile, which needs to happen, so all the custom attributes will be visible
                //PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, string.Join(";", projectDefines.ToArray()));
                EditorDefineSymbolTool.ModifyDefines(projectDefines, new List<string>());

                // Let other systems execute before reloading assemblies
                Thread.Sleep(1000);
                EditorApplication.UnlockReloadAssemblies();
                return;
            }

            //#2 重置：清除所有自定义宏定义
            if (reset)
            {
                foreach (var info in listInfo_All)
                {
                    var define = info.define;
                    projectDefines.Remove(define);
                }
                if (!oldProjectDefines.IsElementEqual(projectDefines))
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(ActiveBuildTargetGroup, projectDefines.ToArray());
                return;
            }

            //#3 新增
            List<string> ccuDefines_ToAdd = listInfo_All.FindAll(i => i.inProject && !i.isActive).ConvertAll(i => i.define);//查找所有在项目但尚未添加的宏定义

            ////#原版实现：会因为顺序不同导致需要刷新
            //var newDefines = string.Join(";", projectDefines.ToArray());
            //if (previousProjectDefines != newDefines)
            //    PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newDefines);
            EditorDefineSymbolTool.ModifyDefines(ccuDefines_ToAdd, new List<string>());
        }

        private static BuildTargetGroup ActiveBuildTargetGroup
        {
            get
            {
                var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
                if (buildTargetGroup == BuildTargetGroup.Unknown)
                {
                    var propertyInfo = typeof(EditorUserBuildSettings).GetProperty("activeBuildTargetGroup", BindingFlags.Static | BindingFlags.NonPublic);
                    if (propertyInfo != null)
                        buildTargetGroup = (BuildTargetGroup)propertyInfo.GetValue(null, null);
                }

                return buildTargetGroup;
            }
        }

        static void ForEachAssembly(Action<Assembly> callback)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    callback(assembly);
                }
                catch (ReflectionTypeLoadException)
                {
                    // Skip any assemblies that don't load properly
                    continue;
                }
            }
        }

        static void ForEachType(Action<Type> callback)
        {
            ForEachAssembly(assembly =>
            {
                var types = assembly.GetTypes();
                foreach (var t in types)
                    callback(t);
            });
        }

        static IEnumerable<Type> GetAssignableTypes(Type type, Func<Type, bool> predicate = null)
        {
            var list = new List<Type>();
            ForEachType(t =>
            {
                if (type.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && (predicate == null || predicate(t)))
                    list.Add(t);
            });

            return list;
        }

        /// <summary>
        /// 对应OptionalDependencyAttribute的信息
        /// </summary>
        public class OptionalDependencyInfo
        {
            public string dependentClass;
            public string define;
            public List<string> listUsedBy = new List<string>();

            //#Runtime
            public bool inProject = false;//是否在此项目中
            public bool isActive = false;//是否已激活
            public OptionalDependencyInfo()
            {
            }

            public OptionalDependencyInfo(string dependentClass, string define)
            {
                this.dependentClass = dependentClass;
                this.define = define;
            }

            public void AddUseBy(string usedBy)
            {
                listUsedBy.Add(usedBy);
            }
        }
    }
}
#endif
