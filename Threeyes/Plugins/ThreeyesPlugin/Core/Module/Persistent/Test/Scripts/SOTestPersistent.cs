using UnityEngine;
namespace Threeyes.Persistent.Test
{
    public class SOTestPersistentBase<TConfig> : ScriptableObject
    {
        public Gradient gradient;
        public TConfig config;
    }

    //[CreateAssetMenu(menuName = "Persistent/Test")]//PS:��Ҫʱ�ټ���
    public class SOTestPersistent : SOTestPersistentBase<TestPersistent.ConfigInfo>
    {
    }
}

