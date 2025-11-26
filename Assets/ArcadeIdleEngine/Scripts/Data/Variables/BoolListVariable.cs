using System.Collections.Generic;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Data.Variables
{
	[CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(Data) + "/" + nameof(Variables) + "/" + nameof(BoolListVariable))]
	public class BoolListVariable : Saveable<List<bool>>
	{
		protected override void OnEnable()
		{
			RuntimeValue = new List<bool>((List<bool>)GetDefaultValue);
		}

		public override void RestoreState(object obj)
		{
			var list = (List<bool>)obj;
			RuntimeValue = new List<bool>(list);
		}

		public void AddElement(bool element)
		{
			RuntimeValue.Add(element);
			OnValueChanged(RuntimeValue);
		}
	}
}
