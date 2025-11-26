using ArcadeBridge.ArcadeIdleEngine.Helpers;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.TweenFeedbacks
{
	[CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(TweenFeedbacks) + "/" + nameof(ShakeScaleFeedback))]
	public class ShakeScaleFeedback : TweenFeedback
	{
		[SerializeField] Vector3 _targetScale;
		
		protected override void OnTweening(Transform trans)
		{
			TweenHelper.ShakeScale(trans, _targetScale, Duration);
		}
	}
}