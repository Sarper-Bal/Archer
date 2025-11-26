using System;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Pools
{
	public class PoolInformer : MonoBehaviour
	{
		public event Action Destroyed;
		
		void OnDestroy()
		{
			Destroyed?.Invoke();
		}
	}
}
