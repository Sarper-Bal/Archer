using ArcadeBridge.ArcadeIdleEngine.Economy;
using ArcadeBridge.ArcadeIdleEngine.Items;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Processors.Sellers
{
	[CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(Processors) + "/" + nameof(Sellers) + "/" + nameof(SellerFloatingImageDefinition))]
	public class SellerFloatingImageDefinition : ScriptableObject
	{
		[field: SerializeField] public FloatingImageResourceAnimator FloatingImageResourceAnimator { get; private set; }
		[field: SerializeField] public ItemDefinition[] SellableItemDefinitions { get; private set; }
		[field: SerializeField, Range(0f, 10f)] public float JumpHeight { get; private set; }
		[field: SerializeField, Range(0.01f, 5f)] public float JumpDuration { get; private set; }
	}

}
