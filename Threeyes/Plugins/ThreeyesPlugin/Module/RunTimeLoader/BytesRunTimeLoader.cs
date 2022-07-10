using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// Unity Bug:通过TextAsset.text可能不能正确获取文本，因此只能通过byte读取（https://forum.unity.com/threads/textasset-text-is-although-the-file-does-have-content.501846/）

/// </summary>
public class BytesRunTimeLoader : RunTimeLoaderBase<byte[]>
{
    protected override byte[] GetAssetFunc(WWW www)
    {
        return www.bytes;
    }
}
