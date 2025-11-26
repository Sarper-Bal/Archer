using System.Collections.Generic;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Data.Database
{
	/// <summary>
	/// List of objects. This list will be used as a remapper for save data. If indexes of 3, 5 and 10 given,
	/// it can return corresponding objects. 
	/// </summary>
	public abstract class ObjectDatabase<TObject> : ScriptableObject where TObject : Object, IDatabaseEntry
	{
		[SerializeField] protected TObject[] _objects;

		void OnEnable()
		{
			for (int i = 0; i < _objects.Length; i++)
			{
				if (_objects[i] == null)
				{
					continue;
				}
				_objects[i].DatabaseIndex = i;
			}
		}

		void OnValidate()
		{
			for (int i = 0; i < _objects.Length; i++)
			{
				if (_objects[i] == null)
				{
					continue;
				}
				_objects[i].DatabaseIndex = i;
			}
		}
		
		public bool GetObjects(List<int> ids, ref List<TObject> result)
		{
			if (ids.Count == 0)
			{
				return false;
			}
			
			for (int i = 0; i < ids.Count; i++)
			{
				result.Add(_objects[ids[i]]);
			}
			return true;
		}
	}
}
