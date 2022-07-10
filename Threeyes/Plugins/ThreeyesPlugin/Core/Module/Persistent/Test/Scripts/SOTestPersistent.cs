using UnityEngine;
namespace Threeyes.Persistent.Test
{
    public class SOTestPersistentBase<TConfig> : ScriptableObject
    {
        public Gradient gradient;
        public TConfig config;
    }

    //[CreateAssetMenu(menuName = "Persistent/Test")]//PS:需要时再激活
    public class SOTestPersistent : SOTestPersistentBase<TestPersistent.ConfigInfo>
    {
    }
}

