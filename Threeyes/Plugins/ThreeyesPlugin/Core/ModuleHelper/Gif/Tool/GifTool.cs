using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class GifTool
{
    public static IEnumerator IEPlayGif(List<GifFrameData> listData, UnityAction<Texture> actUpdateTexture)
    {
        if (listData.Count > 0)
        {
            int startIndex = 0;
            int totalEle = listData.Count;
            while (true)
            {
                actUpdateTexture.Execute(listData[startIndex].texture);
                yield return new WaitForSeconds(listData[startIndex].frameDelaysSeconds);

                startIndex = (startIndex + 1) % totalEle;
            }
        }
    }

}