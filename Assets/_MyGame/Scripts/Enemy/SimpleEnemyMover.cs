using UnityEngine;
using IndianOceanAssets.Engine2_5D; // EnemyStats'a erişim için

namespace ArcadeBridge.ArcadeIdleEngine.Enemy
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(EnemyStats))] 
    public class SimpleEnemyMover : MonoBehaviour
    {
        [Header("Target Settings / Hedef Ayarları")]
        [SerializeField] private string _targetTag = "Player";
        [SerializeField] private float _rotationSpeed = 5f;

        private Transform _target;
        private Rigidbody _rb;
        private EnemyStats _stats;
        
        // Search Timer
        private float _nextSearchTime;
        private const float SEARCH_INTERVAL = 0.5f; 

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _stats = GetComponent<EnemyStats>();

            // Physics Optimization / Fizik Optimizasyonu
            _rb.useGravity = true;
            _rb.isKinematic = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void OnEnable()
        {
            _target = null;
            // Load balancing random start / Yük dengeleme
            _nextSearchTime = Time.time + Random.Range(0f, SEARCH_INTERVAL);
            
            // Reset velocity / Hızı sıfırla
            #if UNITY_6000_0_OR_NEWER
            _rb.linearVelocity = Vector3.zero; 
            #else
            _rb.velocity = Vector3.zero;
            #endif
        }

        private void Update()
        {
            if (_target == null || !_target.gameObject.activeInHierarchy)
            {
                if (_target != null) _target = null; // Clean dead reference

                if (Time.time >= _nextSearchTime)
                {
                    FindTarget();
                    _nextSearchTime = Time.time + SEARCH_INTERVAL; 
                }
            }
        }

        private void FixedUpdate()
        {
            if (_target == null || _stats.Definition == null) return;
            MoveLogic();
        }

        private void FindTarget()
        {
            GameObject targetObj = GameObject.FindGameObjectWithTag(_targetTag);
            if (targetObj != null) 
            {
                _target = targetObj.transform;
            }
        }

        private void MoveLogic()
        {
            Vector3 direction = (_target.position - transform.position).normalized;
            direction.y = 0; 

            if (direction != Vector3.zero)
            {
                // Rotation / Dönüş
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, lookRotation, _rotationSpeed * Time.fixedDeltaTime));

                // Movement / Hareket
                Vector3 velocity = direction * _stats.Definition.MoveSpeed;
                
                #if UNITY_6000_0_OR_NEWER
                velocity.y = _rb.linearVelocity.y;
                _rb.linearVelocity = velocity;
                #else
                velocity.y = _rb.velocity.y;
                _rb.velocity = velocity;
                #endif
            }
        }
    }
}