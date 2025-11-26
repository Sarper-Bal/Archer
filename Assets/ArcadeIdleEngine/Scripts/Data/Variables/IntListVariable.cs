using System.Collections.Generic;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Data.Variables
{
	[CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(Data) + "/" + nameof(Variables) + "/" + nameof(IntListVariable))]
	public class IntListVariable : Saveable<List<int>>
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
			RuntimeValue.Add(element);
			OnValueChanged(RuntimeValue);
		}
	}
}
