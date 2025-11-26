using UnityEngine;
using UnityEngine.EventSystems;

namespace ArcadeBridge.ArcadeIdleEngine.Actors
{
	public class UIJoystickWithWorkingArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
	{
		const float TAP_THRESHOLD = 0.2f;

		[SerializeField] CanvasGroup _background;
		[SerializeField] InputChannel inputChannel;
		[SerializeField] Transform _knobTransform;
		[SerializeField, Range(0f, 1f)] float _pressedOpacity = 0.5f;
		[SerializeField, Range(10f, 500f)] float _maxRange = 50f;
		[SerializeField] bool _horizontalAxisEnabled = true;
		[SerializeField] bool _verticalAxisEnabled = true;
		[SerializeField] bool _isCenterDynamic;

		Vector3 _newJoystickPosition;
		Vector2 _joystickValue;
		Vector2 _neutralPosition;
		float _initialOpacity;
		float _touchTime;

		void Awake()
		{
			_initialOpacity = _background.alpha;
			_knobTransform = _background.transform.GetChild(0);
			inputChannel.SetActiveInput += InputChannelOnSetActiveInput;
		}

		void OnDestroy()
		{
			inputChannel.SetActiveInput -= InputChannelOnSetActiveInput;
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			inputChannel.OnPointerDown();
			SetJoystickPosition();
		}

		public void OnDrag(PointerEventData eventData)
		{
			CalculateAndUpdateInput();
			_touchTime += Time.deltaTime;
			if (_horizontalAxisEnabled || _verticalAxisEnabled)
			{
				inputChannel.OnJoystickUpdated(_joystickValue);
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			inputChannel.OnPointerUp();
			if (_touchTime <= TAP_THRESHOLD)
			{
				inputChannel.OnTap();
			}
			_touchTime = 0f;
			ResetJoystick();
		}

		void InputChannelOnSetActiveInput(bool activeness)
		{
			gameObject.SetActive(activeness);
		}

		void SetJoystickPosition()
		{
			// we define a new neutral position
			_background.transform.position = Input.mousePosition;
			_neutralPosition = Input.mousePosition;

			_background.alpha = _pressedOpacity;
		}

		void CalculateAndUpdateInput()
		{
			Vector2 clampedPosition = Input.mousePosition;

			clampedPosition = Vector2.ClampMagnitude(clampedPosition - _neutralPosition, _maxRange);

			if (!_horizontalAxisEnabled)
			{
				clampedPosition.x = 0;
			}

			if (!_verticalAxisEnabled)
			{
				clampedPosition.y = 0;
			}

			_joystickValue.x = EvaluateInputValue(clampedPosition.x);
			_joystickValue.y = EvaluateInputValue(clampedPosition.y);

			_newJoystickPosition = _neutralPosition + clampedPosition;

			if (_isCenterDynamic)
			{
				_background.transform.position = _newJoystickPosition;
				_neutralPosition = _newJoystickPosition;
			}

			_knobTransform.position = _newJoystickPosition;
		}

		void ResetJoystick()
		{
			_knobTransform.position = _neutralPosition;
			_joystickValue.x = 0f;
			_joystickValue.y = 0f;
			inputChannel.OnJoystickUpdated(_joystickValue);

			_background.alpha = _initialOpacity;
		}

		float EvaluateInputValue(float vectorPosition)
		{
			return Mathf.InverseLerp(0, _maxRange, Mathf.Abs(vectorPosition)) * Mathf.Sign(vectorPosition);
		}
	}
}
