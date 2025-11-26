using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Data.Symbols
{
	[CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(Data) + "/" + nameof(Symbols) + "/" + nameof(Symbol))]
	public class Symbol : ScriptableObject
	{
		[SerializeField] string _value;

		public string Value => _value;
	}
}
