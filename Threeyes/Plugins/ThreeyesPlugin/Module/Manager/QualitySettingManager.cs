using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class QualitySettingManager : InstanceBase<QualitySettingManager>
{
    public List<QualityLevelRange> listQualityLevelRange = new List<QualityLevelRange>();

    public IntEvent onChangeQualityLevel;//更新质量设置（从0开始）

    int curLevel = -1;
    private void Awake()
    {
        curLevel = QualitySettings.GetQualityLevel();
        onChangeQualityLevel.Invoke(curLevel);
    }

    private void Update()
    {
        int level = QualitySettings.GetQualityLevel();
        if (level != curLevel)
        {
            curLevel = level;
            foreach(var qlr in listQualityLevelRange)
            {
                qlr.Detect(curLevel);
            }
            onChangeQualityLevel.Invoke(curLevel);
        }
    }


    [System.Serializable]
    public class QualityLevelRange
    {
        public Range_Int rangeLevel;//质量等级范围
        public BoolEvent onBetweenRange;//是否在范围内
        public UnityEvent onReachMax;
        public UnityEvent onReachMin;
        public BoolEvent onReachMaxMin;

        public void Detect(int curLevel)
        {
            if(curLevel>=rangeLevel.MaxValue)
            {
                onReachMax.Invoke();
                onReachMaxMin.Invoke(true);
                onBetweenRange.Invoke(false);
            }
            else if(curLevel<=rangeLevel.MinValue)
            {
                onReachMin.Invoke();
                onReachMaxMin.Invoke(false);
                onBetweenRange.Invoke(false);
            }
            else
            {
                onBetweenRange.Invoke(true);
            }
        }
    }
}
