#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
namespace Threeyes.Editor
{

    /// <summary>
    /// 给脚本添加和修改命名空间
    /// Todo:要加在#EndIf前
    /// https://blog.csdn.net/pretty_h/article/details/100084084
    /// </summary>
    public class TopMenuEditor_ScriptAddNamespace : ScriptableWizard
    {

        public string folder = "Assets/";
        public string namespaceName;

        void OnEnable()
        {
            if (Selection.activeObject != null)
            {
                string dirPath = AssetDatabase.GetAssetOrScenePath(Selection.activeObject);
                if (File.Exists(dirPath))
                {
                    dirPath = dirPath.Substring(0, dirPath.LastIndexOf("/"));
                }
                folder = dirPath;
            }
        }

        [MenuItem(EditorDefinition.TopMenuItemPrefix + "Script Tool/" + "Add Namespace Window", false, 10)]
        static void CreateWizard()
        {
            TopMenuEditor_ScriptAddNamespace editor = ScriptableWizard.DisplayWizard<TopMenuEditor_ScriptAddNamespace>("Add Namespace", "Add");
            editor.minSize = new Vector2(300, 200);
        }
        public void OnWizardCreate()
        {
            //save settting

            if (!string.IsNullOrEmpty(folder) && !string.IsNullOrEmpty(namespaceName))
            {

                List<string> filesPaths = new List<string>();
                filesPaths.AddRange(
                    Directory.GetFiles(Path.GetFullPath(".") + Path.DirectorySeparatorChar + folder, "*.cs", SearchOption.AllDirectories)
                );
                Dictionary<string, bool> scripts = new Dictionary<string, bool>();

                int counter = -1;
                foreach (string filePath in filesPaths)
                {

                    scripts[filePath] = true;

                    EditorUtility.DisplayProgressBar("Add Namespace", filePath, counter / (float)filesPaths.Count);
                    counter++;

                    string contents = File.ReadAllText(filePath);

                    string result = "";
                    bool havsNS = contents.Contains("namespace ");
                    string t = havsNS ? "" : "\t";

                    using (TextReader reader = new StringReader(contents))
                    {
                        int index = 0;
                        bool addedNS = false;
                        while (reader.Peek() != -1)
                        {
                            string line = reader.ReadLine();

                            if (line.IndexOf("using") > -1 || line.Contains("#"))
                            {
                                result += line + "\n";
                            }
                            else if (!addedNS && !havsNS)
                            {
                                result += "\nnamespace " + namespaceName + " \n{";
                                addedNS = true;
                                result += t + line + "\n";
                            }
                            else
                            {
                                if (havsNS && line.Contains("namespace "))
                                {
                                    if (line.Contains("{"))
                                    {
                                        result += "namespace " + namespaceName + " {\n";
                                    }
                                    else
                                    {
                                        result += "namespace " + namespaceName + "\n";
                                    }
                                }
                                else
                                {
                                    result += t + line + "\n";
                                }
                            }
                            ++index;
                        }
                        reader.Close();
                    }
                    if (!havsNS)
                    {
                        result += "}";
                    }
                    File.WriteAllText(filePath, result);
                }



                //处理加了命名空间后出现方法miss
                filesPaths.AddRange(
                    Directory.GetFiles(Path.GetFullPath(".") + Path.DirectorySeparatorChar + folder, "*.unity", SearchOption.AllDirectories)
                );
                filesPaths.AddRange(
                    Directory.GetFiles(Path.GetFullPath(".") + Path.DirectorySeparatorChar + folder, "*.prefab", SearchOption.AllDirectories)
                );


                counter = -1;
                foreach (string filePath in filesPaths)
                {
                    EditorUtility.DisplayProgressBar("Modify Script Ref", filePath, counter / (float)filesPaths.Count);
                    counter++;

                    string contents = File.ReadAllText(filePath);

                    string result = "";
                    using (TextReader reader = new StringReader(contents))
                    {
                        int index = 0;
                        while (reader.Peek() != -1)
                        {
                            string line = reader.ReadLine();

                            if (line.IndexOf("m_ObjectArgumentAssemblyTypeName:") > -1 && !line.Contains(namespaceName))
                            {

                                string scriptName = line.Split(':')[1].Split(',')[0].Trim();
                                if (scripts.ContainsKey(scriptName))
                                {
                                    line = line.Replace(scriptName, "namespaceName." + scriptName);
                                }

                                result += line + "\n";
                            }
                            else
                            {
                                result += line + "\n";
                            }
                            ++index;
                        }
                        reader.Close();
                    }

                    File.WriteAllText(filePath, result);
                }


                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }
        }
    }
}
#endif