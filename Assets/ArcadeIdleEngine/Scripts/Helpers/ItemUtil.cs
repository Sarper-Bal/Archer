using ArcadeBridge.ArcadeIdleEngine.Items;
using DG.Tweening;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Helpers
{
	public static class ItemUtil
	{
		public static void JumpToDisappearIntoPool(Item item, Vector3 targetPoint, float jumpPower, int numJumps, float duration)
		{
			Sequence sequence = DOTween.Sequence();
			sequence.Append(TweenHelper.Jump(item.transform, targetPoint, jumpPower, numJumps, duration));
			sequence.Append(TweenHelper.DisappearSlowly(item.transform, 0.2f, item.ReleaseToPool));
			sequence.SetAutoKill().SetRecyclable();
			sequence.Play();
		}
	}
}
