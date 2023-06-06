using Newtonsoft.Json;
using UnityEngine;
using Threeyes.Config;

public class AC_ObjectGenerator : ConfigurableComponentBase<AC_SOObjectGeneratorConfig, AC_ObjectGenerator.ConfigInfo>
{
    public int curPrefabIndex = 0;
    protected virtual GameObject GetPrefab()
    {
        if (!Config.soPrefabGroup)
            return null;
        switch (Config.getPrefabType)
        {
            case GetPrefabType.Random:
                return Config.soPrefabGroup.ListData.GetRandom();
            case GetPrefabType.InOrder:
                var result = Config.soPrefabGroup.ListData[GetRepeatIndex(curPrefabIndex)];//Incase index out of bound
                curPrefabIndex = GetRepeatIndex(curPrefabIndex + 1);
                return result;
            default:
                Debug.LogError(Config.getPrefabType + " Not Define!");
                return null;
        }
    }

    int GetRepeatIndex(int originIndex)
    {
        return originIndex % Config.soPrefabGroup.ListData.Count;
    }

    #region Define
    [System.Serializable]
    public class ConfigInfo : SerializableDataBase
    {
        [Header("Generate")]
        [JsonIgnore] public AC_SOPrefabGroup soPrefabGroup;
        public GetPrefabType getPrefabType = GetPrefabType.Random;//How to get new prefab from list
    }

    public enum GetPrefabType
    {
        Random,
        InOrder
    }
    #endregion
}