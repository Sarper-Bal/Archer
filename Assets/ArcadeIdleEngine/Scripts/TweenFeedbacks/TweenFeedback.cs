using ArcadeBridge.ArcadeIdleEngine.Helpers;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.TweenFeedbacks
{
	public abstract class TweenFeedback : ScriptableObject
	{
		[SerializeField, Range(0.02f, 3f)] protected float Duration = 0.5f;

		public void Play(Transform trans)
		{
			TweenHelper.CompleteAll(trans);
			OnTweening(trans);
		}

		protected abstract void OnTweening(Transform trans);
	}
}
