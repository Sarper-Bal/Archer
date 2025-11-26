using ArcadeBridge.ArcadeIdleEngine.Actors;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.LevelGeneration
{
    public class Portal : MonoBehaviour
    {
        LevelGenerator _levelGenerator;
        bool _shouldTeleport;

        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out ArcadeIdleMover arcadeIdleMover))
            {
                if (_shouldTeleport)
                {
                    arcadeIdleMover.transform.position = Vector3.zero;
                }

                _levelGenerator.DestroyLevel();
            }
        }

        public void Initialize(LevelGenerator blg, bool shouldTeleport)
        {
            _levelGenerator = blg;
            _shouldTeleport = shouldTeleport;
        }
    }
}