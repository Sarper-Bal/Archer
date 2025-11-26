using System;
using System.Collections.Generic;
using ArcadeBridge.ArcadeIdleEngine.Helpers;
using ArcadeBridge.ArcadeIdleEngine.Items;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ArcadeBridge.ArcadeIdleEngine.Gathering
{
	[SelectionBase, RequireComponent(typeof(Collider))]
	public class GatherableSource : MonoBehaviour
	{
		[field: SerializeField, Tooltip("Defines a lot of essential properties.")] 
		public GatherableDefinition GatherableDefinition { get; private set; }

		[SerializeField, Tooltip("Feedbacks will be played on this transform component.")]
		Transform _visualTransform;

		[SerializeField, Tooltip("If true, this gameObject will be destroyed after spawning gathering resources.")] 
		bool _destroyAfterGathered;

		List<Item> _instantiatedItems = new List<Item>();
		int _currentHitPoint;

		public bool Depleted { get; private set; }
		
		public event Action<List<Item>> GatheredItemInstantiated;

		void OnDestroy()
		{
			foreach (Item instantiatedItems in _instantiatedItems)
			{
				Destroy(instantiatedItems);
			}
			_instantiatedItems.Clear();
		}

		public void TakeHit(int amount)
		{
			if (_currentHitPoint == GatherableDefinition.MaxHitPoint)
			{
				return;
			}
			
			int newHitPoint = Mathf.Clamp(_currentHitPoint + amount, 0, GatherableDefinition.MaxHitPoint);
			bool isItemSpawned = GatherableDefinition.Gather(_currentHitPoint, newHitPoint, out GatherableReward gatherableReward);
			if (isItemSpawned)
			{
				for (int i = 0; i < gatherableReward.Amount; i++)
				{
					Vector3 randomPoint = Random.insideUnitCircle;
					randomPoint.z = randomPoint.y;
					randomPoint.y = 0f;
					randomPoint = randomPoint.normalized * GatherableDefinition.ItemSpawnRadius;
					Vector3 position = transform.position;
					Item item = gatherableReward.ItemPool.Get();
					item.transform.position = position;
					_instantiatedItems.Add(item);
					TweenHelper.Jump(item.transform, position + randomPoint, 2f, 1, 0.3f);
				}
				GatheredItemInstantiated?.Invoke(_instantiatedItems);
				
				if (GatherableDefinition.UseFeedbacks)
				{
					GatherableDefinition.ItemSpawnedFeedback.Play(_visualTransform);
				}
			}
			else
			{
				if (GatherableDefinition.UseFeedbacks)
				{
					GatherableDefinition.TakeHitFeedback.Play(_visualTransform);
				}
			}
			_currentHitPoint = newHitPoint;
			if (_currentHitPoint == GatherableDefinition.MaxHitPoint)
			{
				Depleted = true;
				TweenHelper.DisappearSlowly(_visualTransform, GatherableDefinition.DisappearingDuration, OnDisappeared);
			}
		}

		void OnDisappeared()
		{
			_visualTransform.gameObject.SetActive(false);
			if (_destroyAfterGathered)
			{
				Destroy(gameObject);
			}
			else
			{
				TweenHelper.DelayedCall(GatherableDefinition.IdleDuration, OnRespawning);
			}
		}

		void OnRespawning()
		{
			TweenHelper.ShowSlowly(_visualTransform, Vector3.one, GatherableDefinition.ReappearDuration, OnRespawned);
		}

		void OnRespawned()
		{
			_currentHitPoint = 0;
			Depleted = false;
		}
	}
}
