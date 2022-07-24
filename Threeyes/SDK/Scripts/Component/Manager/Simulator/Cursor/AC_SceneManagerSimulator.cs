using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AC_SceneManagerSimulator : AC_SceneManagerBase<AC_SceneManagerSimulator>
{
	public const string SimulatorSceneName = "AliveCursorHub_Simulator";
	public const string SimulatorScenePath = "Threeyes/HubSimulator/" + SimulatorSceneName;

	private void Start()
	{
		InitAsync();
	}

	async void InitAsync()
	{
		await Task.Yield();//等待Config初始化完成

		//找到ModScene
		for (int i = 0; i != SceneManager.sceneCount; i++)
		{
			Scene scene = SceneManager.GetSceneAt(i);
			if (scene != hubScene)
			{
				curModScene = scene;
				break;
			}
		}
		if (!curModScene.IsValid())
		{
			Debug.LogError("Please add the Mod Scene before play!");
			return;
		}

		//ToAdd：调用初始化代码
		AC_AliveCursor aliveCursor = curModScene.GetComponents<AC_AliveCursor>().FirstOrDefault();
		if (!aliveCursor)
		{
			Debug.LogError($"Can't find {nameof(AC_AliveCursor)} in Mod Scene!");
			return;
		}
		InitCursor(aliveCursor);
	}
}