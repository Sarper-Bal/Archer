using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Data.Variables
{
	[CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(Data) + "/" + nameof(Variables) + "/" + nameof(BoolVariable))]
	public class BoolVariable : Saveable<bool>
	{
		public override void RestoreState(object obj)
		{
			RuntimeValue = (bool)obj;
		}
		
		protected override void OnEnable()
		{
			RuntimeValue = (bool)GetDefaultValue;
		}
	}
}
