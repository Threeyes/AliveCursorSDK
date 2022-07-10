using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace Threeyes.Data
{
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

        /// <summary>
        /// Copy file from abs path to relate path, also  file to that path
        /// </summary>
        /// <param name="fileAbsPath"></param>
        /// <returns>relate file Path</returns>
        public virtual string CopyToRelatePath(string fileAbsPath)
        {
            try
            {
                string fileName = Path.GetFileName(fileAbsPath);
                string destDirPath = PathTool.GetOrCreateDir(Path.Combine(ParentDir, SubDir));
                string destFilePath = Path.Combine(destDirPath, fileName);

                File.Copy(fileAbsPath, destFilePath, true);//Copy file to relate dir(Todo:移动到UIField_FileBase中）

                fileAbsPath = Path.Combine(SubDir, fileName);//Convert to relate path
            }
            catch (System.Exception e)
            {
                Debug.LogError("Copy file error:\r\n" + e);
            }
            return fileAbsPath;
        }
    }
}
