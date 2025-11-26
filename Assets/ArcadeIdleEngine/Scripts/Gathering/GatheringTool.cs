using System.Collections.Generic;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Gathering
{
	// TODO: Damage and attack speed multipliers
	[SelectionBase]
	public class GatheringTool : MonoBehaviour
	{
		[SerializeField] GatheringToolDefinition _gatheringToolDefinition;

		float _timer;

		public GatheringToolDefinition GatheringToolDefinition
		{
			get => _gatheringToolDefinition;
			set => _gatheringToolDefinition = value;
		}
		public List<GatherableSource> GatherableSources { get; } = new List<GatherableSource>();
		public bool HasInteractableGatherable => GatherableSources.Count != 0;

		void Update()
		{
			_timer += Time.deltaTime;

			if (_timer >= _gatheringToolDefinition.UseInterval)
			{
				_timer -= _gatheringToolDefinition.UseInterval;
				
				foreach (GatherableSource gatherable in GatherableSources)
				{
					gatherable.TakeHit(_gatheringToolDefinition.BaseDamage);
				}
			}
		}

		public void AddGatherable(GatherableSource source)
		{
			GatherableSources.Add(source);
		}
		
		public void RemoveGatherable(GatherableSource source)
		{
			GatherableSources.Remove(source);
		}
	}
}
