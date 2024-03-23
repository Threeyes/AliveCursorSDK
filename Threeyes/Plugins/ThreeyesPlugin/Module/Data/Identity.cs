using System;
using UnityEngine;
#if USE_JsonDotNet
using Newtonsoft.Json;
using UnityEditor;
using Threeyes.Core;
#endif
#if USE_NaughtyAttributes
using NaughtyAttributes;
#endif
/// <summary>
/// 定义组件的必要序列化字段
/// 
/// PS：只是临时定义，后续等RuntimeSceneSerialization完善后就替代为他们的方案
/// </summary>
namespace Threeyes.Data
{
    public interface IIdentityHolder
    {
        public Identity ID { get; set; }
    }

    /// <summary>
    /// 存储指定区间内的唯一ID
    /// 
    /// ToAdd：
    /// -比较接口
    /// </summary>
    [Serializable]
    public struct Identity : IEquatable<Identity>
    {
        public static Identity Empty { get { return new Identity(""); } }
        public bool IsValid { get { return m_Guid.NotNullOrEmpty(); } }
        [JsonIgnore] public string Guid { get => m_Guid; set => m_Guid = value; }
#if USE_NaughtyAttributes
        [AllowNesting]
        [ReadOnly]
#endif
        [SerializeField] string m_Guid;

        [JsonConstructor]//指明使用该Constructor来反序列化时创建类实例（https://www.newtonsoft.com/json/help/html/JsonConstructorAttribute.htm）
        public Identity(string guid)
        {
            m_Guid = guid;
        }

        public Identity(Identity other)
        {
            m_Guid = other.m_Guid;
        }

        /// <summary>
        /// 提供规范生成的GUID
        /// </summary>
        /// <returns></returns>
        public static string NewGuid()
        {
            return System.Guid.NewGuid().ToString();
        }

        #region IEquatable (Ref: ReplayIdentity)
        public override int GetHashCode()
        {
            return m_Guid.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is Identity other)
            {
                return Equals(other);
            }
            return false;
        }
        public bool Equals(Identity other)
        {
            return m_Guid == other.m_Guid;
        }
        public static bool operator ==(Identity a, Identity b)
        {
            return a.Equals(b) == true;
        }
        public static bool operator !=(Identity a, Identity b)
        {
            // Check for not equal
            return a.Equals(b) == false;
        }
        #endregion
        public override string ToString()
        {
            return m_Guid;
        }
    }
}