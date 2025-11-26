using System;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Actors
{
    [CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(Actors) + "/" + nameof(InputChannel))]
    public class InputChannel : ScriptableObject
    {
        public event Action<Vector2> JoystickUpdate;
        public event Action PointerDown;
        public event Action PointerUp;
        public event Action Tap;
        public event Action<bool> SetActiveInput;

        public void OnJoystickUpdated(Vector2 value)
        {
            JoystickUpdate?.Invoke(value);
        }

        public void OnPointerDown()
        {
            PointerDown?.Invoke();
        }

        public void OnPointerUp()
        {
            PointerUp?.Invoke();
        }

        public void OnTap()
        {
            Tap?.Invoke();
        }

        public void OnSetActiveInput(bool activeness)
        {
            SetActiveInput?.Invoke(activeness);
        }
    }
}