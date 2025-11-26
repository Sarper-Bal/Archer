using ArcadeBridge.ArcadeIdleEngine.Helpers;
using ArcadeBridge.ArcadeIdleEngine.Interactables;
using ArcadeBridge.ArcadeIdleEngine.Items;
using ArcadeBridge.ArcadeIdleEngine.Storage;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Processors.Sellers
{
	[SelectionBase]
	public class SellerFloatingImage : MonoBehaviour
	{
		[SerializeField] Inventory _inventory;
		[SerializeField] SellerFloatingImageDefinition _definition;
		[SerializeField] Timer _timer;
		[SerializeField] Transform _sellingPoint;
		
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
			Vector3 point = _camera.WorldToScreenPoint(item.transform.position);
			_definition.FloatingImageResourceAnimator.Play(point, item.Definition.SellValue);
		}
	}
}
