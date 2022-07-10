using System;
using System.Security.Cryptography;
using System.Text;


/// <summary>
/// 加解密及验证签名的工具类
/// </summary>
public static class RSAHelper
{
    /// <summary>
    /// 生成公钥和私钥
    /// </summary>
    /// <param name="privateKey">私匙</param>
    /// <param name="publicKey">公匙></param>
    public static void GenerateKeys(out string privateKey, out string publicKey)
    {
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
        {
            privateKey = rsa.ToXmlString(true);
            publicKey = rsa.ToXmlString(false);
        }
    }

    #region 签名验证 https://www.cnblogs.com/slyzly/articles/6786592.html

    /// <summary>
    /// 生成RSA签名
    /// </summary>
    /// <param name="sSource" >明文</param>
    /// <param name="sPrivateKey" >私匙</param>
    /// <returns>密文</returns>
    public static string CreateSignature(string sSource, string sPrivateKey)
    {
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
        {
            rsa.FromXmlString(sPrivateKey);

            RSAPKCS1SignatureFormatter sf = new RSAPKCS1SignatureFormatter(rsa);
            sf.SetHashAlgorithm("SHA256");

            byte[] source = System.Text.ASCIIEncoding.ASCII.GetBytes(sSource);

            SHA256Managed sha2 = new SHA256Managed();
            byte[] result = sha2.ComputeHash(source);
            byte[] signature = sf.CreateSignature(result);

            return Convert.ToBase64String(signature);
        }
    }

    /// <summary>
    /// RSA签名验证
    /// </summary>
    /// <param name="sEncryptSource">密文</param>
    /// <param name="sSource">需要比较的明文字符串</param>
    /// <param name="sPublicKey">公匙</param>
    /// <returns>是否相同</returns>
    public static bool VerifySignature(string sEncryptSource, string sSource, string sPublicKey)
    {
        try
        {
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
                rsa.FromXmlString(sPublicKey);

                RSAPKCS1SignatureDeformatter df = new RSAPKCS1SignatureDeformatter(rsa);
                df.SetHashAlgorithm("SHA256");

                byte[] signature = Convert.FromBase64String(sEncryptSource);

                SHA256Managed sha2 = new SHA256Managed();
                byte[] compareByte = sha2.ComputeHash(ASCIIEncoding.ASCII.GetBytes(sSource));

                return df.VerifySignature(compareByte, signature);
            }
        }
        catch (Exception)
        {
            return false;
        }
    }

    #endregion

    #region 对称加密/解密（缺点：有长度限制）

    /// <summary> 
    /// RSA加密数据 
    /// </summary> 
    /// <param name="express">要加密数据</param> 
    /// <param name="KeyContainerName">密匙容器的名称</param> 
    /// <returns></returns> 
    public static string RSAEncryption(string express, string KeyContainerName)
    {

        CspParameters param = new CspParameters();
        param.KeyContainerName = KeyContainerName; //密匙容器的名称，保持加密解密一致才能解密成功
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(param))
        {
            byte[] plaindata = Encoding.Default.GetBytes(express);//将要加密的字符串转换为字节数组
            byte[] encryptdata = rsa.Encrypt(plaindata, false);//将加密后的字节数据转换为新的加密字节数组
            return Convert.ToBase64String(encryptdata);//将加密后的字节数组转换为字符串
        }
    }
    /// <summary> 
    /// RSA解密数据 
    /// </summary>
    /// <param name="ciphertext">要解密数据</param> 
    /// <param name="KeyContainerName">密匙容器的名称</param> 
    /// <returns></returns> 
    public static string RSADecrypt(string ciphertext, string KeyContainerName)
    {
        CspParameters param = new CspParameters();
        param.KeyContainerName = KeyContainerName; //密匙容器的名称，保持加密解密一致才能解密成功
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(param))
        {
            byte[] encryptdata = Convert.FromBase64String(ciphertext);
            byte[] decryptdata = rsa.Decrypt(encryptdata, false);
            return System.Text.Encoding.Default.GetString(decryptdata);
        }
    }

    #endregion
}
