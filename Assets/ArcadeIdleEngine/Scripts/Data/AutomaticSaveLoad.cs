using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Data
{
	public class AutomaticSaveLoad : MonoBehaviour
	{
		[SerializeField] SaveManager _saveManager;
		[SerializeField] bool _saveAutomatically = true;

		void Start()
		{
			_saveManager.RestoreAll();
			DontDestroyOnLoad(this);
		}
		
		void OnApplicationPause(bool pauseStatus)
		{
			if (!_saveAutomatically)
			{
				return;
			}
			if (pauseStatus)
			{
				_saveManager.SaveAll();	
			}
		}

		void OnApplicationQuit()
		{
			if (!_saveAutomatically)
			{
				return;
			}
			_saveManager.SaveAll();
		}
	}
}
