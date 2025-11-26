using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Editor
{
	public static class SaveDirectory
	{
		[MenuItem("Tools/ArcadeIdleEngine/Open Save Directory")]
		static void OpenSaveDirectory()
		{
			string savePath = Application.persistentDataPath;

			if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			{
				System.Diagnostics.Process.Start("open", $"-R \"{savePath}\"");
			}
			else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			{
				Application.OpenURL(savePath);
			}
		}
	}
}
