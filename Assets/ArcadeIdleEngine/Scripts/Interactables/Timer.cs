using System;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Interactables
{
	// TODO: Add Time.deltaTime to here instead of every other classes
	public class Timer : MonoBehaviour
	{
		public struct EventArgs
		{
			public float Value;
			public float MaxValue;

			public EventArgs(float value, float maxValue)
			{
				Value = value;
				MaxValue = maxValue;
			}
		}
		
		[SerializeField] float _duration = 0.25f;
		
		float _value;

		/// <summary>
		/// Event contains max value and current value for the timer.
		/// </summary>
		public event Action<EventArgs> ValueChanged;

		public bool IsCompleted => _value >= _duration;

		public void Add(float amount)
		{
			_value += amount;
			ValueChanged?.Invoke(new EventArgs(_value, _duration));
		}

		/// <summary>
		/// Adds the duration that it takes to go from previous frame to current one. 
		/// </summary>
		public void Tick()
		{
			Add(Time.deltaTime);
		}

		public void SetZero()
		{
			_value = 0f;
			ValueChanged?.Invoke(new EventArgs(_value, _duration));
		}
	}
}
