using Threeyes.EventPlayer;
using UnityEngine;

public class TestAEP : MonoBehaviour
{
    public float scaler = 1;
    public EventPlayer_SOAction actionEventPlayer;
    public void TestPlay(bool isPlay)
    {
        actionEventPlayer.PlayWithParam(isPlay, scaler);
    }
}
