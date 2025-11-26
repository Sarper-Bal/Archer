using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Booting
{
    public class FrameRateUnlocker : MonoBehaviour
    {
        [SerializeField] int _targetFrameRate = 60;

        void Awake()
        {
            Application.targetFrameRate = _targetFrameRate;
        }
    }
}