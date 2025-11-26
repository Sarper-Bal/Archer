using System;
using ArcadeBridge.ArcadeIdleEngine.Helpers;
using ArcadeBridge.ArcadeIdleEngine.Items;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Storage
{
    [Serializable]
	public class InventoryInvisible : InventoryBase
	{
		[SerializeField] int _capacity;
		
		public override bool IsFull() => Count >= _capacity;

		public void SetCapacity(int capacity)
		{
			_capacity = capacity;
		}

		protected override void OnAdding(Item item)
		{
			Transform trans = item.transform;
			TweenHelper.KillAllTweens(trans);
			trans.SetParent(StackingPoint);
			TweenHelper.LocalJumpAndRotate(item.transform, Vector3.zero, Vector3.zero, 2f, PickUpDuration);
			TweenHelper.ScaleDownSlowlyInBack(item.transform, PickUpDuration);
			//item.JumpAndDisappear(Vector3.zero, 2f, PickUpDuration);
		}
	}
}
