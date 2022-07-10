using UnityEngine;
/// <summary>
/// 标注Enum的对应值
/// （PS：如需多选枚举，请直接使用Flag标记）
/// </summary>
public class EnumValueAttribute : PropertyAttribute
{
    public string enumName;

    //Value int different type
    public string strValue;
    public int intValue;
    public float floatValue;
    public object objValue;

    public EnumValueAttribute() { }

    public EnumValueAttribute(string name)
    {
        enumName = name;
    }

    public EnumValueAttribute(string name, object value)
    {
        enumName = name;
        objValue = value;
    }

    public EnumValueAttribute(string name, string value)
    {
        enumName = name;
        strValue = value;
    }

    public EnumValueAttribute(string name, int value)
    {
        enumName = name;
        intValue = value;
    }

    public EnumValueAttribute(string name, float value)
    {
        enumName = name;
        floatValue = value;
    }

}