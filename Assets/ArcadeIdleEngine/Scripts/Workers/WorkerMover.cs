using ArcadeBridge.ArcadeIdleEngine.Interactables;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

namespace ArcadeBridge.ArcadeIdleEngine.Workers
{
    /// <summary>
    /// Patrols between two points, if there is an item it picks and carries them to the target place and 
    /// waits in there for Y second. Can be used to simulate "smart" AI.
    /// </summary>
    public class WorkerMover : MonoBehaviour, IInteractor
    {
        [SerializeField] NavMeshAgent _navMeshAgent;

        Vector3 _loadPoint;
        Vector3 _unloadPoint;
        float _loadTime;
        float _unloadTime;

        bool _isCurrentDestinationLoad;

        bool IInteractor.Interactable => !enabled;

        void Update()
        {
            if (_navMeshAgent.remainingDistance < 0.1f)
            {
                if (_isCurrentDestinationLoad)
                {
                    DOVirtual.DelayedCall(_loadTime, Resume, false);
                }
                else
                {
                    DOVirtual.DelayedCall(_unloadTime, Resume, false);
                }
                _isCurrentDestinationLoad = !_isCurrentDestinationLoad;
                enabled = false;
            }
        }

        public void Initialize(Vector3 loadPoint, Vector3 unloadPoint, float loadTime, float unloadTime)
        {
            _loadPoint = loadPoint;
            _unloadPoint = unloadPoint;
            _loadTime = loadTime;
            _unloadTime = unloadTime;
            _isCurrentDestinationLoad = true;
            
            Resume();
        }

        public void Pause()
        {
            _navMeshAgent.ResetPath();
            enabled = false;
        }

        public void Resume()
        {
            enabled = true;
            if (_isCurrentDestinationLoad)
            {
                _navMeshAgent.SetDestination(_loadPoint);
            }
            else
            {
                _navMeshAgent.SetDestination(_unloadPoint);
            }
        }
    }
}