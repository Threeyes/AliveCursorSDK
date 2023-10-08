using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Threeyes.Data
{
    /// <summary>
    /// 用于中转、保存FilePathModifier字段
    /// </summary>
    public interface IFilePathModifierHolder
    {
        FilePathModifier FilePathModifier { get; set; }
    }

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

        /// <summary>
        /// 如果传入值是相对路径，就转为全局路径；否则返回输入路径
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public virtual string GetAbsPath(string filePath)
        {
            return PathTool.GetAbsPath(ParentDir, filePath);
        }
    }
}
