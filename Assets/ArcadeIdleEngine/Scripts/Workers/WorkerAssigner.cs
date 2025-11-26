using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Workers
{
    public class WorkerAssigner : MonoBehaviour
    {
        [SerializeField] WorkerManager _workerManager;
        [SerializeField] Transform _instantiatingPoint;
        [SerializeField] Vector3 _loadPoint;
        [SerializeField] Vector3 _unloadTarget;
        [SerializeField] float _loadTime = 2f;
        [SerializeField] float _unloadTime = 2f;
        
        public void Assign()
        {
            _workerManager.AssignEmployee(_loadPoint, _unloadTarget, _loadTime, _unloadTime, _instantiatingPoint.position);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(_loadPoint, 1f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_unloadTarget, 1f);
        }
    }
}