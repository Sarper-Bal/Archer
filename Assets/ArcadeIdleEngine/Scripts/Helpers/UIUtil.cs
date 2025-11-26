using DG.Tweening;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Helpers
{
	public static class UIUtil
	{
		public static void MoveUI(Transform trans, Vector3 target, float duration, TweenCallback onComplete)
		{
			Sequence sequence = DOTween.Sequence().SetRecyclable();
			sequence.Append(trans.DOMove(target, duration * 0.3f));
			sequence.Join(trans.DOScale(Vector3.one, duration * 0.3f));
			sequence.Append(trans.DOScale(Vector3.one / 2f, duration).SetEase(Ease.InBack, 5f));
			sequence.Join(trans.DOLocalMove(Vector3.zero, duration * 0.7f).SetEase(Ease.InQuart));
			sequence.AppendCallback(onComplete);
		}
	}
}
