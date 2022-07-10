using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AC_SystemAudioManagerBase<T> : AC_ManagerBase<T>
	, IAC_SystemAudioManager
	, IAC_Manager_ModInitHandler
	, IAC_CommonSetting_IsCursorActiveHandler
where T : AC_SystemAudioManagerBase<T>
{
	#region Interface
	public int RawSampleCount { get { return rawSampleCount; } }
	public int FFTCount { get { return fftSize; } }
	public int SpectrumCount { get { return spectrumCount; } }
	protected const int rawSampleCount = 128;//源数据(256足够呈现波动，太多会有割裂感）
	protected const int fftSize = 128;// 【受音量影响】Defines FFT data size constants that can be used for FFT calculations. (Note that only the half of the specified size can be used for visualizations.)   
	protected const int spectrumCount = 128;//柱状图（64已经足够细致，因为光标显示区域不大，不应该过于细分）

	/// <summary>
	/// Get average DB
	/// </summary>
	/// <param name="rawSampleData"></param>
	/// <returns></returns>
	public float CalculateLoudness(float[] rawSampleData)
	{
		float v = 0f,
			len = rawSampleData.Length;

		for (int i = 0; i < len; i++)
			v += Mathf.Abs(rawSampleData[i]);//PS:因为值的范围为[-1,1]，所以要取绝对值

		//Root mean square is a good approximation of perceived loudness: (https://answers.unity.com/questions/157940/getoutputdata-and-getspectrumdata-they-represent-t.html)
		return Mathf.Sqrt(v / (float)len);
	}
	#endregion

	#region Property & Field
	///PS：
	///1.存储的Data是左/右声的平均值
	protected float[] rawSampleData;//(Data range: [-1, 1])（值为类似Sin函数的Y值。原理：https://answers.unity.com/questions/472188/what-does-getoutputdata-sample-float-represent.html）[PS:不应该暴露rawData，否则容易获取信息]
	protected float[] fftData;//(Data range: [0.0, 1.0])
	protected float[] spectrumData;//(Data range: [0.0, 1.0])
	#endregion

	#region Unity Method

	protected virtual void Awake()
	{
		rawSampleData = new float[RawSampleCount];
		fftData = new float[FFTCount];
		spectrumData = new float[SpectrumCount];
	}
	#endregion

	#region Callback
	public virtual void OnModInit(Scene scene, AC_AliveCursor aliveCursor)
	{
		//ToAdd:Hub中搜索实现接口的组件数量，如果为0，则该组件不更新，节省资源
	}
	public virtual void OnModDeinit(Scene scene, AC_AliveCursor aliveCursor)
	{
	}

	protected bool commonSetting_IsCursorActive = true;
	public void OnIsCursorActiveChanged(bool isActive)
	{
		commonSetting_IsCursorActive = isActive;
	}

	#endregion

	#region Inner Method

	protected bool isDataLocked = false;
	protected virtual void OnDataChanged()
	{
		// Since this is being changed on a seperate thread we do this to be safe
		lock (spectrumData)
		{
			isDataLocked = true;
			//临时注释
			AC_EventCommunication.SendMessage<IAC_SystemAudio_RawSampleDataChangedHandler>((inst) => inst.OnRawSampleDataChanged(rawSampleData));
			AC_EventCommunication.SendMessage<IAC_SystemAudio_FFTDataChangedHandler>((inst) => inst.OnFFTDataChanged(fftData));
			AC_EventCommunication.SendMessage<IAC_SystemAudio_SpectrumDataChangedHandler>((inst) => inst.OnSpectrumDataChanged(spectrumData));

			isDataLocked = false;
		}
	}

	#endregion
}
//测试