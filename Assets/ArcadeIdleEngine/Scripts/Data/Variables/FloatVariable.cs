using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Data.Variables
{
	[CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(Data) + "/" + nameof(Variables) + "/" + nameof(FloatVariable))]
	public class FloatVariable : Saveable<float>
	{
		protected override void OnEnable()
		{
			RuntimeValue = (float)GetDefaultValue;
		}
		
		public override void RestoreState(object obj)
		{
			RuntimeValue = (float)obj;
		}
	}
}
