using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Threeyes.Data
{
	/// <summary>
	/// Modify input Path
	/// </summary>
    public class FilePathModifier
    {
        public string ParentDir { get; set; }
        public string SubDir { get; set; }

        public FilePathModifier(string parentDir, string subDir)
        {
            ParentDir = parentDir;
            SubDir = subDir;
        }
        public virtual string GetAbsPath(string filePath)
        {
            //如果传入值是相对路径，就转为全局路径
            return PathTool.GetAbsPath(ParentDir, filePath);
        }
    }
}
