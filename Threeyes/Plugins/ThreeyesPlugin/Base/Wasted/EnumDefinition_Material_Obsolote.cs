/// <summary>
/// 更换材质的类型
/// </summary>
public enum MaterialTweenType_Obsolete
{
    AlphaOrEmission = 0,//第四个值，如普通的Color为Alpha，Emission为亮度
    Color = 10,
    Value = 20,
    Offset = 110,
}

/// <summary>
/// 更换材质值的类型
/// </summary>
public enum MaterialValueType_Obsolete
{
    _Custom = -1,//自定义的名称
    _Cutoff = 0,//Alpha Cutoff
    _BumpScale = 5,//Normal Map Scale
}

/// <summary>
/// 更换颜色的类型
/// </summary>
public enum MaterialColorType_Obsolete
{
    _Custom = -1,//自定义的名称
    _Color = 0,
    _EmissionColor = 1
}

/// <summary>
/// 更换贴图的类型
/// </summary>
public enum MaterialTextureType_Obsolete
{
    _Custom = -1,//自定义的名称
    _MainTex = 0,
    _EmissionMap = 1
}