using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using Threeyes.Data;
using Threeyes.Config;
using UnityEngine;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// Creeper response To Audio
    /// 
    /// 功能：
    /// -通过序号确定指定响应音频的Leg
    /// </summary>
    public class CreeperAudioVisualizer : ConfigurableComponentBase<SOCreeperAudioVisualizerConfig, CreeperAudioVisualizer.ConfigInfo>
    , IHubSystemAudio_RawSampleDataChangedHandler
    {

        public CreeperTransformController creeperGhostController;
        public List<CreeperLegController> listCreeperLegGhostController { get { return creeperGhostController.listLegController; } }

        #region Callback
        /// <summary>
        /// PS:
        /// -只有当前有音频数据时才会进入
        /// </summary>
        /// <param name="data"></param>
        public void OnRawSampleDataChanged(float[] data)
        {
            float volume = AudioVisualizerTool.CalculateLoudness(data);
            Vector3 axisPercent = Vector3.zero;//偏转实现：将输入值分成三等分，分别对应XYZ的旋转缩放值

            //PS:因为data range: [-1.0, 1.0]，刚好适用于正负随机旋转值
            if (data.Length < 3)
                return;
            int numPerSubArray = data.Length / 3;//取小值

            for (int i = 0; i != numPerSubArray; i++)
                axisPercent.x += data[i];
            for (int i = numPerSubArray; i != 2 * numPerSubArray; i++)
                axisPercent.y += data[i];
            for (int i = 2 * numPerSubArray; i != 3 * numPerSubArray; i++)
                axisPercent.z += data[i];
            //Debug.Log(rotatePercent / numPerSubArray + "/" + volume+"="+ rotatePercent / numPerSubArray/ volume);
            axisPercent /= (numPerSubArray);
            if (volume > 0)
                axisPercent /= volume;//消除音量大小造成的振幅衰减

            ///Body
            ///-Sync with rhythm
            creeperGhostController.tfBodyMixer.localPosition = Config.canBodyMove ? Config.bodyMoveRange.Multi(axisPercent) : Vector3.zero;
            creeperGhostController.tfBodyMixer.localEulerAngles = Config.canBodyRotate ? Config.bodyRotateRange.Multi(axisPercent) : Vector3.zero;

            ///Legs       
            ///-Raise Leg
            var legControllerToRaise = GetLegControllerToRaise();//获取需要抬脚的Controller
            listCreeperLegGhostController.ForEach(c =>
            {
                if (!c.isMoving)//只有脚不移动时才能更改该值
                {
                    //c.CompWeight = listDesireLegController.Contains(c) ? volume * Config.legRaiseRange + (1 - Config.legRaiseRange) : 1;//volume reach max== Weight is 1 (模拟随着节拍跺脚，音量最大对应脚落下。缺点是暂停播放时脚一直抬着，弃用。)

                    c.CompWeight = legControllerToRaise == c ? 1 - volume * Config.legRaiseRange : 1;//volume reach min== Weight is 1
                }
            });
        }

        CreeperLegController GetLegControllerToRaise()
        {
            if (Config.canLegRaise)
            {
                int legRaiseIndex = Config.legRaiseIndex;
                if (legRaiseIndex >= 0 && legRaiseIndex < listCreeperLegGhostController.Count)
                {
                    return listCreeperLegGhostController[legRaiseIndex];
                }
            }
            return null;
        }
        #endregion

        #region Define
        /// <summary>
        /// 
        /// ToUpdate:
        /// -使用Callback，避免频繁判断及获取Leg等操作，以及在切换Leg后重置其他Leg
        /// -增加限制legRaiseIndex的方法（利用RangeExAttribute，然后通过方法返回maxLegCount对应值）
        /// </summary>
        [Serializable]
        public class ConfigInfo : SerializableDataBase
        {
            public bool canBodyMove = false;
            public Vector3 bodyMoveRange = new Vector3(0.05f, 0.05f, 0);
            public bool canBodyRotate = true;
            public Vector3 bodyRotateRange = new Vector3(5, 0, 5);
            public bool canLegRaise = false;
            [Range(0.1f, 1)] public float legRaiseRange = 1f;
            [RangeEx(nameof(LegRaiseMinIndex), nameof(LegRaiseMaxIndex), nameof(UseRange))] public int legRaiseIndex = 0;//Which leg to raise

            [Header("Set by Modder")]
            [JsonIgnore] public int totalLegCount = 2;//[Modder] Set the totalLeg （暂时放此处，因为无法访问其他Controller的Info）

            [JsonIgnore] bool UseRange { get { return true; } }
            [JsonIgnore] float LegRaiseMinIndex { get { return 0; } }
            [JsonIgnore] float LegRaiseMaxIndex { get { return totalLegCount - 1; } }
        }
        #endregion
    }
}