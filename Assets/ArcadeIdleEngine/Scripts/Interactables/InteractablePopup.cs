using ArcadeBridge.ArcadeIdleEngine.Actors;
using ArcadeBridge.ArcadeIdleEngine.Helpers;
using UnityEngine;
using UnityEngine.Animations;

namespace ArcadeBridge.ArcadeIdleEngine.Interactables
{
	public class InteractablePopup : MonoBehaviour
	{
		[SerializeField] Canvas _canvas;
		[SerializeField] LookAtConstraint _lookAtConstraint;

		void Awake()
		{
			ConstraintSource source = new ConstraintSource();
			source.sourceTransform = Camera.main.transform;
			source.weight = 1f;
			_lookAtConstraint.AddSource(source);
		}

		void OnTriggerEnter(Collider other)
		{
			if (other.TryGetComponent(out ArcadeIdleMover _))
			{
				_lookAtConstraint.enabled = true;
				TweenHelper.ShowSlowly(_canvas.transform, Vector3.one, 0.5f, null);
			}
		}
		
		void OnTriggerExit(Collider other)
		{
			if (other.TryGetComponent(out ArcadeIdleMover _))
			{
				_lookAtConstraint.enabled = false;
				TweenHelper.DisappearSlowlyAndDeactivate(_canvas.transform);
			}
		}
	}
}
