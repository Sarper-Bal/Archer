using ArcadeBridge.ArcadeIdleEngine.Actors;
using DG.Tweening;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Interactables
{
	public class LidOpener : MonoBehaviour
	{
		[SerializeField] Transform _lidTransform;

		void OnTriggerEnter(Collider other)
		{
			if (other.TryGetComponent(out ArcadeIdleMover _))
			{
				_lidTransform.DOLocalRotate(Vector3.right * 90f, 0.5f);
			}
		}
		
		void OnTriggerExit(Collider other)
		{
			if (other.TryGetComponent(out ArcadeIdleMover _))
			{
				_lidTransform.DOLocalRotate(Vector3.zero, 0.5f);
			}
		}
	}
}
