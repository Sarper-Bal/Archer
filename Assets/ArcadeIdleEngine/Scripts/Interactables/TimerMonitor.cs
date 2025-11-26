using System;
using UnityEngine;
using UnityEngine.UI;

namespace ArcadeBridge.ArcadeIdleEngine.Interactables
{
	public class TimerMonitor : MonoBehaviour
	{
		[SerializeField] Timer _timer;
		[SerializeField] Image _image;

		void OnEnable()
		{
			_timer.ValueChanged += Timer_ValueChanged;
		}

		void OnDisable()
		{
			_timer.ValueChanged -= Timer_ValueChanged;
		}
		
		void Timer_ValueChanged(Timer.EventArgs eventArgs)
		{
			_image.fillAmount = eventArgs.Value / eventArgs.MaxValue;
		}
	}
}
