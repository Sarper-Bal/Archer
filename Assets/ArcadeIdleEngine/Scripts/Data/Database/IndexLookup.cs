using System.Collections.Generic;
using ArcadeBridge.ArcadeIdleEngine.Data.Variables;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ArcadeBridge.ArcadeIdleEngine.Data.Database
{
	public abstract class IndexLookup<T> : ScriptableObject where T : Object, IDatabaseEntry
	{
		[SerializeField] ObjectDatabase<T> _objectDatabase;
		[SerializeField] UniqueIntListVariable _savedIndex;
		[SerializeField] List<T> _availableObjects = new List<T>();

		public List<T> AvailableObjects => _availableObjects;

		void OnEnable()
		{
			_savedIndex.ValueSet += SavedIndex_ValueSet;
			_availableObjects.Clear();
			if (_savedIndex.RuntimeValue != null)
			{
				_objectDatabase.GetObjects(_savedIndex.RuntimeValue, ref _availableObjects);
			}
		}

		void OnDisable()
		{
			_savedIndex.ValueSet -= SavedIndex_ValueSet;
			_availableObjects.Clear();
		}

		void SavedIndex_ValueSet(List<int> obj)
		{
			_availableObjects.Clear();
			_objectDatabase.GetObjects(_savedIndex.RuntimeValue, ref _availableObjects);
		}
	}
}
