using System;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Experimental
{
	public class SimpleLogger : MonoBehaviour
	{
		[SerializeField] string _message;

		void Start()
		{
			Debug.Log(_message);
		}
	}
}
