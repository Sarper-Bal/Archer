using ArcadeBridge.ArcadeIdleEngine.Data;
using ArcadeBridge.ArcadeIdleEngine.Data.Variables;
using ArcadeBridge.ArcadeIdleEngine.Economy;
using ArcadeBridge.ArcadeIdleEngine.Items;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Processors.Sellers
{
	[CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(Processors) + "/" + nameof(Sellers) + "/" + nameof(SellerFloatingTextDefinition))]
	public class SellerFloatingTextDefinition : ScriptableObject
	{
		[field: SerializeField] public IntVariable IncomeResource { get; private set; }
		[field: SerializeField] public FloatingTextResourceAnimator FloatingTextResourceAnimator { get; private set; }
		[field: SerializeField] public ItemDefinition[] SellableItemDefinitions { get; private set; }
		[field: SerializeField, Range(0f, 10f)] public float JumpHeight { get; private set; }
		[field: SerializeField, Range(0.01f, 5f)] public float JumpDuration { get; private set; }
	}
}
