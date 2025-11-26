using System;
using ArcadeBridge.ArcadeIdleEngine.Pools;
using ArcadeBridge.ArcadeIdleEngine.TweenFeedbacks;
using ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Core.MetaAttributes;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Gathering
{
	[CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(Gathering) + "/" + nameof(GatherableDefinition))]
	public class GatherableDefinition : ScriptableObject
	{
		[field: SerializeField, Tooltip("Contains hit points and amount of reward that should be spawned.")]
		public HitPointThreshold HitPoints { get; private set; }
		
		[field: SerializeField, Tooltip("After successful gathering, this type will be spawned as a reward.")] 
		public ItemPool GatheringOutputItemPool { get; private set; }
		
		[field: SerializeField, Tooltip("Reward items will be spawned around the source in this radius.")]
		public float ItemSpawnRadius { get; private set; }
		
		[field: SerializeField, Range(0.05f, 1f), Tooltip("Duration of object scale goes to zero in seconds")]
		public float DisappearingDuration { get; private set; } = 0.5f;
		
		[field: SerializeField, Range(0.1f, 2f), Tooltip("Duration of object scale goes to one in seconds")]
		public float ReappearDuration { get; private set; } = 1f;
		
		[field: SerializeField, Min(0.02f), Tooltip("Object wait duration before its scale goes to one in seconds")]
		public float IdleDuration { get; private set; } = 3f;
		
		[field: SerializeField] 
		public bool UseFeedbacks { get; private set; }
		
		[field: SerializeField, ShowIf(nameof(UseFeedbacks))] 
		public TweenFeedback ItemSpawnedFeedback  { get; private set; }
		
		[field: SerializeField, ShowIf(nameof(UseFeedbacks))] 
		public TweenFeedback TakeHitFeedback { get; private set; }

		public int MaxHitPoint => HitPoints.GetMaxHitPoint;

		void OnEnable()
		{
			HitPoints.Initialize();
		}

		/// <summary>
		/// Takes current hit point and damage amount, then calculates whether it produces new items as a reward
		/// </summary>
		/// <returns></returns>
		public bool Gather(int currentHitPoint, int newHitPoint, out GatherableReward gatherableReward)
		{
			gatherableReward = new GatherableReward();
			int spawnCount = HitPoints.GetOutputCount(currentHitPoint, newHitPoint);
			if (spawnCount == 0)
			{
				return false;
			}
			gatherableReward.ItemPool = GatheringOutputItemPool;
			gatherableReward.Amount = spawnCount;
			return true;
		}
	}
	public struct GatherableReward
	{
		public ItemPool ItemPool;
		public int Amount;
	}

	[Serializable]
	public class HitPointThreshold
	{
		[SerializeField] 
		Threshold[] _value;
		
		Threshold[] _calculatedValue;
		
		int _totalHitPoint;
		
		public int GetMaxHitPoint => _calculatedValue[_calculatedValue.Length - 1].HitPoint;

		public void Initialize()
		{
			_totalHitPoint = 0;
			if (_value == null)
			{
				return;
			}
			_calculatedValue = new Threshold[_value.Length];
			for (int i = 0; i < _value.Length; i++)
			{
				Threshold threshold = _value[i];
				_totalHitPoint += threshold.HitPoint;
				_calculatedValue[i].HitPoint = _totalHitPoint;
				_calculatedValue[i].ItemSpawnCount = threshold.ItemSpawnCount;
			}
		}

		/// <summary>
		/// Takes damage and based on the threshold it lost it returns reward count
		/// </summary>
		/// <returns>Item count that needs to be spawn</returns>
		public int GetOutputCount(int from, int to)
		{
			int firstIndex = -1;
			int lastIndex = -1;
			for (int i = 0; i < _calculatedValue.Length; i++)
			{
				if (from >= _calculatedValue[i].HitPoint)
				{
					firstIndex = i;
				}

				if (to >= _calculatedValue[i].HitPoint)
				{
					lastIndex = i;
				}
			}
			int delta = lastIndex - firstIndex;
			int totalReward = 0;
			for (int i = 1; i <= delta; i++)
			{
				totalReward += _calculatedValue[firstIndex + i].ItemSpawnCount;
			}
			return totalReward;
		}

		[Serializable]
		struct Threshold
		{
			public int HitPoint;
			public int ItemSpawnCount;
		}
	}
}
