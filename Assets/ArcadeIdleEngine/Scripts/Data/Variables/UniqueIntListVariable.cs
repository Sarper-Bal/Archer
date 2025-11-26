using System.Collections.Generic;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Data.Variables
{
	[CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(Data) + "/" + nameof(Variables) + "/" + nameof(UniqueIntListVariable))]
	public class UniqueIntListVariable : Saveable<List<int>>
	{
		protected override void OnEnable()
		{
			RuntimeValue = new List<int>((List<int>)GetDefaultValue);
		}

		public override void RestoreState(object obj)
		{
			var list = (List<int>)obj;
			RuntimeValue = new List<int>(list);
		}

		public void AddElement(int element)
		{
			if (RuntimeValue.Contains(element))
			{
				Debug.LogError($"You are trying to add element {element} twice into the {name} list");
				return;
			}
			
			RuntimeValue.Add(element);
			OnValueChanged(RuntimeValue);
		}
		
		public bool Contains(int element)
		{
			return RuntimeValue.Contains(element);
		}
	}
}
