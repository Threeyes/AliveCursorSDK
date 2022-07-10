using System;
using UnityEngine;
/// <summary>
/// 优点：没有长度限制
/// </summary>
public static class AESHelper
{

    /// <summary>
    /// 默认密钥-密钥的长度必须是32
    /// </summary>
    private const string defaultPublicKey = "1234567890123456";

    /// <summary>
    /// 默认向量
    /// </summary>
    private const string Iv = "colyuwonderfulpd";

    /// <summary>
    /// AES加密
    /// </summary>
    /// <param name="str">需要加密的字符串</param>
    /// <param name="key">32位密钥</param>
    /// <returns>加密后的字符串</returns>
    public static string Encrypt(string str, string key)
    {
        try
        {
            Byte[] keyArray = System.Text.Encoding.UTF8.GetBytes(key);
            Byte[] toEncryptArray = System.Text.Encoding.UTF8.GetBytes(str);
            var rijndael = new System.Security.Cryptography.RijndaelManaged();
            rijndael.Key = keyArray;
            rijndael.Mode = System.Security.Cryptography.CipherMode.ECB;
            rijndael.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            rijndael.IV = System.Text.Encoding.UTF8.GetBytes(Iv);
            System.Security.Cryptography.ICryptoTransform cTransform = rijndael.CreateEncryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }
        catch (Exception e)
        {
            Debug.LogError("AES加密失败！\r\n" + e);
            return null;
        }
    }

    /// <summary>
    /// AES解密
    /// </summary>
    /// <param name="str">需要解密的字符串</param>
    /// <param name="key">32位密钥</param>
    /// <returns>解密后的字符串</returns>
    public static string Decrypt(string str, string key)
    {
        try
        {
            Byte[] keyArray = System.Text.Encoding.UTF8.GetBytes(key);
            Byte[] toEncryptArray = Convert.FromBase64String(str);
            var rijndael = new System.Security.Cryptography.RijndaelManaged();
            rijndael.Key = keyArray;
            rijndael.Mode = System.Security.Cryptography.CipherMode.ECB;
            rijndael.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            rijndael.IV = System.Text.Encoding.UTF8.GetBytes(Iv);
            System.Security.Cryptography.ICryptoTransform cTransform = rijndael.CreateDecryptor();
            Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return System.Text.Encoding.UTF8.GetString(resultArray);
        }
        catch (Exception e)
        {
            Debug.LogError("AES解密失败！\r\n" + e);
            return null;
        }
    }
}
