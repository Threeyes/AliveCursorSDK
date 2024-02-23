using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.Core
{
    public static class DateTimeTool
    {
        public static string fileFormat = "yyyyMMdd_HHmmss";//适用于文件名
        public static string defaultFormat = "yyyy-MM-dd HH:mm:ss";//适用于Editor编辑、DateTime.TryParse/TryDeSerializeDateTime转换
    }
}