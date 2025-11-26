using ArcadeBridge.ArcadeIdleEngine.Data.Database;
using ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Core.MetaAttributes;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Gathering
{
	[CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(Gathering) + "/" + nameof(GatheringToolDefinition))]
	public class GatheringToolDefinition : ScriptableObject, IDatabaseEntry
	{
		[field: SerializeField, ReadOnly] public int DatabaseIndex { get; set; }

		[SerializeField] GatheringToolType gatheringToolType;
		[SerializeField] Sprite _toolSprite;
		[SerializeField] GatheringTool _gatheringToolPrefab;
		[SerializeField] int _tier;
		[SerializeField] int _baseDamage = 10;
		[SerializeField] float _useInterval = 1f;
		[SerializeField] GatherableDefinition[] _gatherables;

		public GatherableDefinition[] GatherableDefinitions => _gatherables;
		public GatheringTool GatheringToolPrefab => _gatheringToolPrefab;
		public int InteractionAnimationId => gatheringToolType.InteractionAnimationId;
		public int BaseDamage => _baseDamage;
		public int Tier => _tier;
		public float UseInterval => _useInterval;
		
		public bool CanGather(GatherableDefinition source)
		{
			foreach (GatherableDefinition definition in _gatherables)
			{
				if (definition == source)
				{
					return true;
				}
			}
			return false;
		}

		void OnValidate()
		{
			if (_gatheringToolPrefab)
			{
				_gatheringToolPrefab.GatheringToolDefinition = this;
			}
		}
	}
}
