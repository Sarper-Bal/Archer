using System;
using ArcadeBridge.ArcadeIdleEngine.Helpers;
using ArcadeBridge.ArcadeIdleEngine.Interactables;
using ArcadeBridge.ArcadeIdleEngine.Items;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Storage
{
    [Serializable]
	public class InventoryVisible : InventoryBase
	{
		[SerializeField] RowColumnHeight _rowColumnHeight;
		
		public override bool IsFull() => Count >= _rowColumnHeight.GetCapacity();

		public void SetCapacity(RowColumnHeight rowColumnHeight)
		{
			_rowColumnHeight = rowColumnHeight;
		}
		
		protected override void OnAdding(Item item)
		{
			Vector3 targetPos = ArcadeIdleHelper.GetPoint(Count, _rowColumnHeight);
			Transform trans = item.transform;
			TweenHelper.KillAllTweens(trans);
			trans.SetParent(StackingPoint);
			TweenHelper.LocalJumpAndRotate(trans, targetPos, Vector3.zero, 2f, PickUpDuration);
		}
		
		protected override void OnRemoved(Item item)
		{
			// Adjust the positions of other items
			int indexOf = Items.IndexOf(item);
			if (Items.Count >= indexOf + 1)
			{
				for (int i = indexOf + 1; i < Items.Count; i++)
				{
					Vector3 targetPoint = ArcadeIdleHelper.GetPoint(i, _rowColumnHeight);
					TweenHelper.KillAllTweens(Items[i].transform);
					TweenHelper.LocalMove(Items[i].transform, targetPoint, 0.1f);
				}
			}
		}
	}
}
