using System.Collections.Generic;

namespace ArcadeBridge.ArcadeIdleEngine.Data
{
	public class SaveData
	{
		public int Version;
		public Dictionary<string, object> Saves = new Dictionary<string, object>();
	}
}