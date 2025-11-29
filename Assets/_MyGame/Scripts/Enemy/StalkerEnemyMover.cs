using UnityEngine;
using IndianOceanAssets.Engine2_5D;

namespace ArcadeBridge.ArcadeIdleEngine.Experimental
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(EnemyStats))]
    public class StalkerEnemyMover : MonoBehaviour
    {
        private enum StalkerState { Searching, MovingToLastPos }

        [Header("Hareket Ayarları")]
        [SerializeField] private string _targetTag = "Player";
        [SerializeField] private float _rotationSpeed = 8f;
        [SerializeField] private float _arrivalDistance = 0.5f;

        [Header("Debug")]
        [SerializeField] private bool _showDebugGizmos = true;

        private Rigidbody _rb;
        private EnemyStats _stats;
        private Transform _realTargetTransform;
        private StalkerState _currentState;
        private Vector3 _lastKnownPosition;
        private float _arrivalDistanceSqr; 
        private float _nextSearchTime;
        private const float SEARCH_INTERVAL = 1.0f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _stats = GetComponent<EnemyStats>();
            _rb.useGravity = true;
            _rb.isKinematic = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
            _arrivalDistanceSqr = _arrivalDistance * _arrivalDistance;
        }

        private void OnEnable()
        {
            _currentState = StalkerState.Searching;
            _nextSearchTime = Time.time;
            
            #if UNITY_6000_0_OR_NEWER
            _rb.linearVelocity = Vector3.zero;
            #else
            _rb.velocity = Vector3.zero;
            #endif
        }

        private void Update()
        {
            switch (_currentState)
            {
                case StalkerState.Searching: HandleSearchingState(); break;
                case StalkerState.MovingToLastPos: CheckArrival(); break;
            }
        }

        private void FixedUpdate()
        {
            if (_currentState == StalkerState.MovingToLastPos) MoveToPosition(_lastKnownPosition);
        }

        private void HandleSearchingState()
        {
            if (_realTargetTransform != null && _realTargetTransform.gameObject.activeInHierarchy)
            {
                LockNewDestination();
                return;
            }
            if (Time.time >= _nextSearchTime)
            {
                FindTargetInScene();
                _nextSearchTime = Time.time + SEARCH_INTERVAL;
            }
        }

        private void CheckArrival()
        {
            float dx = transform.position.x - _lastKnownPosition.x;
            float dz = transform.position.z - _lastKnownPosition.z;
            float distSqr = (dx * dx) + (dz * dz);

            if (distSqr <= _arrivalDistanceSqr)
            {
                #if UNITY_6000_0_OR_NEWER
                _rb.linearVelocity = Vector3.zero;
                #else
                _rb.velocity = Vector3.zero;
                #endif
                _currentState = StalkerState.Searching;
            }
        }

        private void LockNewDestination()
        {
            _lastKnownPosition = _realTargetTransform.position;
            _currentState = StalkerState.MovingToLastPos;
        }

        private void FindTargetInScene()
        {
            GameObject targetObj = GameObject.FindGameObjectWithTag(_targetTag);
            if (targetObj != null)
            {
                _realTargetTransform = targetObj.transform;
                LockNewDestination();
            }
        }

        private void MoveToPosition(Vector3 destination)
        {
            if (_stats.Definition == null) return;

            Vector3 direction = (destination - transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, lookRotation, _rotationSpeed * Time.fixedDeltaTime));

                Vector3 moveVelocity = direction * _stats.Definition.MoveSpeed;
                
                #if UNITY_6000_0_OR_NEWER
                moveVelocity.y = _rb.linearVelocity.y;
                _rb.linearVelocity = moveVelocity;
                #else
                moveVelocity.y = _rb.velocity.y;
                _rb.velocity = moveVelocity;
                #endif
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_showDebugGizmos) return;
            if (_currentState == StalkerState.MovingToLastPos)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(_lastKnownPosition, _arrivalDistance);
                Gizmos.DrawLine(transform.position, _lastKnownPosition);
            }
        }
        
        private void OnValidate()
        {
            _arrivalDistanceSqr = _arrivalDistance * _arrivalDistance;
        }
        
        // [SİLİNDİ] OnCollisionEnter ve ExplodeAndDie kaldırıldı.
    }
}