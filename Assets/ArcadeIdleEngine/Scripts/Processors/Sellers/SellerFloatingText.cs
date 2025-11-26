using ArcadeBridge.ArcadeIdleEngine.Helpers;
using ArcadeBridge.ArcadeIdleEngine.Interactables;
using ArcadeBridge.ArcadeIdleEngine.Items;
using ArcadeBridge.ArcadeIdleEngine.Storage;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Processors.Sellers
{
	[SelectionBase]
    public class SellerFloatingText : MonoBehaviour
	{
		[SerializeField] SellerFloatingTextDefinition _definition;
		[SerializeField] Inventory _inventory;
		[SerializeField] Transform _sellingPoint;
		[SerializeField] Timer _timer;

		Camera _camera;
		
		void Awake()
		{
			_camera = Camera.main;
		}

		void Update()
		{
			if (_inventory.IsEmpty)
			{
				return;
			}
			// If inventory is empty do nothing. If it's not, then if it has an item that we can sell, increase the timer and sell it.
			foreach (ItemDefinition sellable in _definition.SellableItemDefinitions)
			{
				if (!_inventory.Contains(sellable, out Item result))
				{
					continue;
				}
				
				
				if (_timer.IsCompleted)
				{
					_inventory.Remove(result);
					TweenHelper.KillAllTweens(result.transform);
					TweenHelper.Jump(result.transform, _sellingPoint.position, _definition.JumpHeight, 1, _definition.JumpDuration, () => Sell(result));
					_timer.SetZero();
				}
				else
				{
					_timer.Tick();
				}
				return;
			}
		}
		
		void Sell(Item item)
		{
			item.ReleaseToPool();
			int itemSellValue = item.Definition.SellValue;
			_definition.IncomeResource.RuntimeValue += itemSellValue;
			_definition.FloatingTextResourceAnimator.Play(transform, _camera.transform, itemSellValue);
		}
	}
}
