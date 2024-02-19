using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.Steamworks
{
    public static class AudioVisualizerTool
    {
        /// <summary>
        /// Calculate average DB from giving data
        /// 
        /// Todo：参数增加start及end（归一化进度），方便进行子集采样
        /// </summary>
        /// <param name="rawSampleData"></param>
        /// <returns>Value between [0,1]</returns>
        public static float CalculateLoudness(float[] rawSampleData, float startPercent = 0, float endPercent = 1)
        {
            float values = 0f;
            int rawDataLength = rawSampleData.Length;

            int startIndex = (int)(Mathf.Clamp01(startPercent) * rawDataLength);
            int endIndex = (int)(Mathf.Clamp01(endPercent) * rawDataLength);
            int subLength = endIndex - startIndex;

            for (int i = 0; i < rawDataLength; i++)
                values += Mathf.Abs(rawSampleData[i]);//先累加每个采样点的音量。PS：因为音频源数据是通过三角函数曲线转化为来，值的范围为[-1,1]，所以要取绝对值

            //Root mean square is a good approximation of perceived loudness: (https://answers.unity.com/questions/157940/getoutputdata-and-getspectrumdata-they-represent-t.html)
            float result = Mathf.Sqrt(values / rawDataLength);
            if (result < volumeThreshold)//即使当前无音频输出，数字仍不为0（如0.0003)，此时需要进行裁剪
                result = 0;
            return result;
        }

        const float volumeThreshold = 0.005f;//多少音量才当作有效
    }
}