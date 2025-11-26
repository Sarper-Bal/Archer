using ArcadeBridge.ArcadeIdleEngine.Data;
using ArcadeBridge.ArcadeIdleEngine.Data.Variables;
using ArcadeBridge.ArcadeIdleEngine.Pools;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Economy
{
	/// <summary>
	/// Spawned gameObject that will move on the screen. E.g. after selling an item, money icon appears on the screen
	/// that goes to the money UI.
	/// </summary>
	public class AnimatedResourceEntity : MonoBehaviour
	{
		IntVariable _intVariable;
		AnimatedResourceEntityPool _animatedResourceEntityPool;
		ResourceTargetImage _resourceTargetImage;
		int _resourceAmount;

		public void Initialize(IntVariable var, int resourceAmount, ResourceTargetImage targetImage, AnimatedResourceEntityPool entityPool, Vector3 startingPosition)
		{
			_intVariable = var;
			_resourceAmount = resourceAmount;
			_animatedResourceEntityPool = entityPool;
			_resourceTargetImage = targetImage;

			Transform trans = transform;
			trans.SetParent(targetImage.transform);
			trans.position = startingPosition;
			trans.localScale = Vector3.zero;
		}

		public void OnMoveSequenceEnded()
		{
			_intVariable.RuntimeValue += _resourceAmount;
			transform.localScale = Vector3.one;
			_animatedResourceEntityPool.Release(this);
			_resourceTargetImage.PlayFeedback();
		}
	}
}
