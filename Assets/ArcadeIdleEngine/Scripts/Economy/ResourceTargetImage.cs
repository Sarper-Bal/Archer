using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace ArcadeBridge.ArcadeIdleEngine.Economy
{
	/// <summary>
	/// Sets target image for animated resources so they arrive at the right position.
	/// </summary>
	public class ResourceTargetImage : MonoBehaviour
	{
		[SerializeField] FloatingImageResourceAnimator floatingImageResourceAnimator;
		
		[SerializeField, Tooltip("Target image for animated resources to arrive.")] 
		Image _resourceImageScalable;

		Tween _tween;

		void Awake()
		{
			_tween = _resourceImageScalable.transform.DOPunchScale(Vector3.one * 0.7f, 0.3f, 10, 0f).Pause().SetAutoKill(false);
		}

		void OnEnable()
		{
			floatingImageResourceAnimator.ResourceTargetImage = this;
		}

		void OnDisable()
		{
			floatingImageResourceAnimator.ResourceTargetImage = null;
		}
		
		public void PlayFeedback()
		{
			_tween.Restart();
		}
	}
}
