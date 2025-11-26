using DG.Tweening;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Booting
{
	public class LoadingScreenTween : MonoBehaviour
	{
		Sequence _sequence;
		void Start()
		{
			_sequence = DOTween.Sequence();
			_sequence.Append(transform.DOScale(new Vector3(1.3f, 0.8f, 1f), 1.2f).SetEase(Ease.InCubic));
			_sequence.Append(transform.DOLocalMove(Vector3.up * 150f, 0.6f).SetEase(Ease.OutCubic).SetRelative());
			_sequence.Join(transform.DOScale(new Vector3(0.7f, 2f, 1f), 0.6f)).SetEase(Ease.OutCubic);
			_sequence.Append(transform.DOLocalMove(Vector3.up * -150f, 0.6f).SetEase(Ease.InCubic).SetRelative());
			_sequence.Join(transform.DOScale(new Vector3(1f, 1f, 1f), 0.6f)).SetEase(Ease.InCubic);
			_sequence.SetLoops(-1, LoopType.Restart);
			_sequence.Play();
		}

		void OnDestroy()
		{
			_sequence.Kill();
		}
	}
}
