using ArcadeBridge.ArcadeIdleEngine.Data;
using ArcadeBridge.ArcadeIdleEngine.Data.Variables;
using ArcadeBridge.ArcadeIdleEngine.Helpers;
using ArcadeBridge.ArcadeIdleEngine.Pools;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ArcadeBridge.ArcadeIdleEngine.Economy
{
	/// <summary>
	/// Creates animated resource entities and moves them on the screen.
	/// </summary>
	[CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(Economy) + "/" + nameof(FloatingImageResourceAnimator))]
	public class FloatingImageResourceAnimator : ScriptableObject
	{
		[SerializeField, Tooltip("GameObject pool that will spawn on the screen when resource amount changed.")] 
		AnimatedResourceEntityPool animatedResourceEntityPool;
		
		[SerializeField, Tooltip("Actual resource amount we have. This will be changed when resource modified.")] 
		IntVariable _resource;
		
		[SerializeField, Range(0f, 5f), Tooltip("Total duration for the animation effect in seconds.")] 
		float _visualizationDuration;

		public ResourceTargetImage ResourceTargetImage { get; set; }

		public void Play(Vector3 screenSpacePosition, int resourceAmount)
		{
			AnimatedResourceEntity pooledImage = animatedResourceEntityPool.Get();
			pooledImage.Initialize(_resource, resourceAmount, ResourceTargetImage, animatedResourceEntityPool, screenSpacePosition);
			Transform trans = pooledImage.transform;
			UIUtil.MoveUI(trans, trans.position + (Vector3)Random.insideUnitCircle * 100f, _visualizationDuration, pooledImage.OnMoveSequenceEnded);
		}
	}
}
