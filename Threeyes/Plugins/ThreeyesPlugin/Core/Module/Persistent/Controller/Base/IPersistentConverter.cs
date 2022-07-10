using System;
namespace Threeyes.Persistent
{
    public interface IPersistentConverter
    {
        string Serialize<T>(T obj);
        T Deserialize<T>(string content, Type realType);
        T Deserialize<T>(string content);
    }
}