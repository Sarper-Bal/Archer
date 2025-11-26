using ArcadeBridge.ArcadeIdleEngine.Experimental;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace ArcadeBridge.ArcadeIdleEngine.Editor
{
	[InitializeOnLoad]
	public static class BootSceneLoader
	{
		const string LOAD_BOOTING_SCENE_FIRST_KEY = "LoadBootingSceneFirst";
		const string LOAD_BOOTING_SCENE_FIRST_MENU_NAME = "Tools/ArcadeIdleEngine/Load Booting Scene First";

		static string currentScenePath;

		static BootSceneLoader()
		{
			EditorOnlyDebugger.Log("load booting scene first key value: " + EditorPrefs.GetBool(LOAD_BOOTING_SCENE_FIRST_KEY, true));
			if (EditorPrefs.GetBool(LOAD_BOOTING_SCENE_FIRST_KEY, true))
			{
				SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneUtility.GetScenePathByBuildIndex(0));
				EditorSceneManager.playModeStartScene = scene;
			}
			else
			{
				EditorSceneManager.playModeStartScene = null;
			}
		}

		[MenuItem(LOAD_BOOTING_SCENE_FIRST_MENU_NAME)]
		static void Toggle()
		{
			EditorPrefs.SetBool(LOAD_BOOTING_SCENE_FIRST_KEY, !EditorPrefs.GetBool(LOAD_BOOTING_SCENE_FIRST_KEY, true));
			if (EditorPrefs.GetBool(LOAD_BOOTING_SCENE_FIRST_KEY, true))
			{
				SceneAsset scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(SceneUtility.GetScenePathByBuildIndex(0));
				EditorSceneManager.playModeStartScene = scene;
			}
			else
			{
				EditorSceneManager.playModeStartScene = null;
			}
		}
		
		[MenuItem(LOAD_BOOTING_SCENE_FIRST_MENU_NAME, true)]
		static bool ToggleValidate()
		{
			Menu.SetChecked(LOAD_BOOTING_SCENE_FIRST_MENU_NAME, EditorPrefs.GetBool(LOAD_BOOTING_SCENE_FIRST_KEY, true));
			return true;
		}

		[MenuItem("Tools/ArcadeIdleEngine/Load Booting Scene")]
		static void LoadBootingSceneEditor()
		{
			EditorSceneManager.OpenScene(SceneUtility.GetScenePathByBuildIndex(0), OpenSceneMode.Single);
		}
	}
}
