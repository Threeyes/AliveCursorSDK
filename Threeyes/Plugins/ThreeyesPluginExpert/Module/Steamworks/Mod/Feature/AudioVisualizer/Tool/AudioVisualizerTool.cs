using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.Steamworks
{
    public static class AudioVisualizerTool
    {
        /// <summary>
        /// Get average DB
        /// </summary>
        /// <param name="rawSampleData"></param>
        /// <returns></returns>
        public static float CalculateLoudness(float[] rawSampleData)
        {
            float v = 0f,
                len = rawSampleData.Length;

            for (int i = 0; i < len; i++)
                v += Mathf.Abs(rawSampleData[i]);//PS:因为值的范围为[-1,1]，所以要取绝对值

            //Root mean square is a good approximation of perceived loudness: (https://answers.unity.com/questions/157940/getoutputdata-and-getspectrumdata-they-represent-t.html)
            return Mathf.Sqrt(v / (float)len);
        }
    }
}