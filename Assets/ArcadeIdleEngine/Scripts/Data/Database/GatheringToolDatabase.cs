using ArcadeBridge.ArcadeIdleEngine.Gathering;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Data.Database
{
	/// <summary>
	/// List of gathering tools. This list will be used as a remapper for save data. If indexes of 3, 5 and 10 given,
	/// it can return corresponding gathering tools. 
	/// </summary>
	[CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(Data) + "/" + nameof(Database) + "/" + nameof(GatheringToolDatabase))]
	public class GatheringToolDatabase : ObjectDatabase<GatheringToolDefinition>
	{
		
	}
}
