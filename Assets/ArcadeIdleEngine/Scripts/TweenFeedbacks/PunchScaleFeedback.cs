using ArcadeBridge.ArcadeIdleEngine.Helpers;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.TweenFeedbacks
{
	[CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(TweenFeedbacks) + "/" + nameof(PunchScaleFeedback))]
	public class PunchScaleFeedback : TweenFeedback
	{
		[SerializeField] Vector3 _targetScale;
		
		protected override void OnTweening(Transform trans)
		{
			TweenHelper.CompleteAll(trans);
			TweenHelper.PunchScale(trans, _targetScale, Duration);
		}
	}
}
