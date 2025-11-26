using ArcadeBridge.ArcadeIdleEngine.Items;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Processors.Spawners
{
	[CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(Processors) + "/" + nameof(Spawners)+ "/" + nameof(SpawnerDefinition))]
	public class SpawnerDefinition : ScriptableObject
	{
		[field: SerializeField] public ItemDefinition SpawningItem { get; set; }
	}
}
