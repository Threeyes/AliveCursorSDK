using System;
using UnityEngine;

/// <summary>
/// 自定义多语言显示名称
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
public class NameAttribute : Attribute
{
    public string Name { get; set; }
    public string languageCode = "zh";//多语言

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <param name="languageCode">（https://www.loc.gov/standards/iso639-2/php/code_list.php）</param>
    public NameAttribute(string name, string languageCode = "zh")
    {
        Name = name;
        this.languageCode = languageCode;
    }
}
