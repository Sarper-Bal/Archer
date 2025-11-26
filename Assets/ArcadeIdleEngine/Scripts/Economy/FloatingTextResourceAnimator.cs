using ArcadeBridge.ArcadeIdleEngine.Pools;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Economy
{
	[CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(Economy) + "/" + nameof(FloatingTextResourceAnimator))]
	public class FloatingTextResourceAnimator : ScriptableObject
	{
		[field: SerializeField] public TextPool TextPool { get; private set; }
		[field: SerializeField] public Vector2 TextFloatingRandomRange { get; private set; }
		[field: SerializeField, Range(0f, 10f)] public float FloatingDuration { get; private set; }

		
		public void Play(Transform feedbackStartTransform, Transform cameraTransform, int increaseAmount)
		{
			TextMeshPro txt = TextPool.Get();
			txt.text = increaseAmount.ToString();
			txt.transform.position = feedbackStartTransform.position + Vector3.up;
			txt.transform.LookAt(cameraTransform.position + cameraTransform.forward * 500f);
			float rndX = Random.Range(-TextFloatingRandomRange.x, TextFloatingRandomRange.x);
			txt.transform.DOMove(txt.transform.position + Vector3.up * TextFloatingRandomRange.y + feedbackStartTransform.right * rndX, FloatingDuration)
				.SetRecyclable()
				.OnComplete(() => TextPool.Release(txt));
		}
	}
}
