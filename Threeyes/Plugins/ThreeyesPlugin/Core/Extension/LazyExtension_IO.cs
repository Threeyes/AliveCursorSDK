using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Threeyes.Core
{
    public static class LazyExtension_IO
    {
        /// <summary>
        /// Ref:https://social.msdn.microsoft.com/forums/vstudio/en-US/815d5586-2587-4a17-bee1-c6d06000ad68/getfiles-all-picture-files
        /// 用法：SearchByExtensions(false, "jpg", "jpeg","png", "bmp", "tiff", "tif", "gif");
        /// </summary>
        /// <param name="directoryInfo"></param>
        /// <param name="recursive"></param>
        /// <param name="extensions"></param>
        /// <returns></returns>
        public static List<FileInfo> SearchByExtensions(this DirectoryInfo directoryInfo, bool recursive, params string[] extensions)
        {
            bool all = extensions == null || extensions.Length == 0;
            Regex regexExtensions = null;
            if (!all)
            {
                StringBuilder regexBuilder = new StringBuilder(@"^.+\.(");
                regexBuilder.Append(string.Join("|", extensions));
                regexBuilder.Append(")$");

                regexExtensions = new Regex(regexBuilder.ToString(), RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
            return directoryInfo.GetFiles("*.*", recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
               .Where(fi => all || regexExtensions.IsMatch(fi.FullName))
               .ToList();
        }
    }
}