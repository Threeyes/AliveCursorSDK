/// <summary>
/// �������ʵ�����
/// </summary>
public enum MaterialTweenType_Obsolete
{
    AlphaOrEmission = 0,//���ĸ�ֵ������ͨ��ColorΪAlpha��EmissionΪ����
    Color = 10,
    Value = 20,
    Offset = 110,
}

/// <summary>
/// ��������ֵ������
/// </summary>
public enum MaterialValueType_Obsolete
{
    _Custom = -1,//�Զ��������
    _Cutoff = 0,//Alpha Cutoff
    _BumpScale = 5,//Normal Map Scale
}

/// <summary>
/// ������ɫ������
/// </summary>
public enum MaterialColorType_Obsolete
{
    _Custom = -1,//�Զ��������
    _Color = 0,
    _EmissionColor = 1
}

/// <summary>
/// ������ͼ������
/// </summary>
public enum MaterialTextureType_Obsolete
{
    _Custom = -1,//�Զ��������
    _MainTex = 0,
    _EmissionMap = 1
}