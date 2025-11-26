using ArcadeBridge.ArcadeIdleEngine.Data.Symbols;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Data.Variables
{
	[CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(Data) + "/" + nameof(Variables) + "/" + nameof(SymbolicFloat))]
	public class SymbolicFloatVariable : Saveable<SymbolicFloat>
	{
		public override void RestoreState(object obj)
		{
			RuntimeValue = (SymbolicFloat)obj;
		}
		
		protected override void OnEnable()
		{
			RuntimeValue = (SymbolicFloat)GetDefaultValue;
		}
	}
}
