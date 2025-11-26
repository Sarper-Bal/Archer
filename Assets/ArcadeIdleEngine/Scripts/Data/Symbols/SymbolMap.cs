using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Data.Symbols
{
	[CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(Data) + "/" + nameof(Symbols) + "/" + nameof(SymbolMap))]
	public class SymbolMap : ScriptableObject
	{
		[SerializeField] Symbol[] _symbols;

		public string GetSymbolizedValue(float coefficient, int symbolIndex)
		{
			return coefficient.ToString("F2") + _symbols[symbolIndex].Value;
		}
		
		public string GetSymbolizedValue(SymbolicFloat symbolicFloat)
		{
			return symbolicFloat.Number.ToString("F2") + _symbols[symbolicFloat.Symbol].Value;
		}
	}
}
