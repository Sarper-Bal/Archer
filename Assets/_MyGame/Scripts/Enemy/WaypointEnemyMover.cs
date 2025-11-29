using UnityEngine;
using IndianOceanAssets.Engine2_5D; // EnemyStats için

namespace ArcadeBridge.ArcadeIdleEngine.Enemy
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(EnemyStats))]
    public class WaypointEnemyMover : MonoBehaviour
    {
        [Header("Yol Ayarları")]
        [SerializeField] private WaypointRoute _route; // Hangi yolu izleyecek?
        [SerializeField] private float _rotationSpeed = 8f;
        [SerializeField] private float _arrivalDistance = 0.5f;
        [SerializeField] private bool _loopPath = true; // Yol bitince başa dönsün mü?

        [Header("Debug")]
        [SerializeField] private bool _showDebugLogs = false;

        // Bileşenler
        private Rigidbody _rb;
        private EnemyStats _stats;

        // Mantık Değişkenleri
        private int _currentWaypointIndex = 0;
        private Transform _currentTargetPoint;
        private float _arrivalDistanceSqr;
        private bool _isPathComplete = false;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _stats = GetComponent<EnemyStats>();

            // Fizik Ayarları (Optimizasyon)
            _rb.useGravity = true;
            _rb.isKinematic = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;

            // [OPTİMİZASYON] Karekök işleminden kaçınmak için
            _arrivalDistanceSqr = _arrivalDistance * _arrivalDistance;
        }

        private void OnEnable()
        {
            ResetPath();
        }

        private void FixedUpdate()
        {
            if (_isPathComplete || _currentTargetPoint == null || _stats.Definition == null) 
            {
                // Dur
                #if UNITY_6000_0_OR_NEWER
                _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0);
                #else
                _rb.velocity = new Vector3(0, _rb.velocity.y, 0);
                #endif
                return;
            }

            MoveAlongPath();
            CheckArrival();
        }

        private void MoveAlongPath()
        {
            // Hedefe giden vektör (Y eksenini yoksay)
            Vector3 direction = (_currentTargetPoint.position - transform.position).normalized;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                // Dönüş
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, lookRotation, _rotationSpeed * Time.fixedDeltaTime));

                // Hareket
                Vector3 moveVelocity = direction * _stats.Definition.MoveSpeed;

                #if UNITY_6000_0_OR_NEWER
                moveVelocity.y = _rb.linearVelocity.y; // Yerçekimini koru
                _rb.linearVelocity = moveVelocity;
                #else
                moveVelocity.y = _rb.velocity.y;
                _rb.velocity = moveVelocity;
                #endif
            }
        }

        private void CheckArrival()
        {
            // [OPTİMİZASYON] sqrMagnitude kullanımı
            float dx = transform.position.x - _currentTargetPoint.position.x;
            float dz = transform.position.z - _currentTargetPoint.position.z;
            float distSqr = (dx * dx) + (dz * dz);

            if (distSqr <= _arrivalDistanceSqr)
            {
                AdvanceToNextWaypoint();
            }
        }

        private void AdvanceToNextWaypoint()
        {
            _currentWaypointIndex++;

            // Listenin sonuna geldik mi?
            if (_currentWaypointIndex >= _route.Waypoints.Count)
            {
                if (_loopPath)
                {
                    _currentWaypointIndex = 0; // Başa dön
                    if (_showDebugLogs) Debug.Log($"{name}: Yol bitti, başa dönülüyor.");
                }
                else
                {
                    _isPathComplete = true; // Dur
                    _currentTargetPoint = null;
                    if (_showDebugLogs) Debug.Log($"{name}: Yol tamamlandı.");
                    return;
                }
            }

            _currentTargetPoint = _route.Waypoints[_currentWaypointIndex];
        }

        // --- PUBLIC API (Spawnerlar kullanabilir) ---
        
        /// <summary>
        /// Düşmana dışarıdan yeni bir rota atamak için kullanılır.
        /// </summary>
        public void SetRoute(WaypointRoute newRoute)
        {
            _route = newRoute;
            ResetPath();
        }

        private void ResetPath()
        {
            _isPathComplete = false;
            _currentWaypointIndex = 0;

            if (_route != null && _route.Waypoints.Count > 0)
            {
                _currentTargetPoint = _route.Waypoints[0];
                // İsteğe bağlı: Düşmanı direkt ilk noktaya ışınla
                // transform.position = _currentTargetPoint.position; 
            }
            else
            {
                _isPathComplete = true; // Rota yoksa hareket etme
            }
        }
        
        // Çarpışma ve Hasar Mantığı (Diğer scriptlerle aynı)
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                if (_stats.Definition != null)
                {
                     if (collision.gameObject.TryGetComponent(out IDamageable damageable))
                     {
                         damageable.TakeDamage(_stats.Definition.ContactDamage);
                         
                         // Efekt ve Ölüm (Pooling)
                         if (_stats.Definition.DeathEffectPool != null)
                         {
                             var effect = _stats.Definition.DeathEffectPool.Get();
                             effect.transform.position = transform.position;
                             effect.Initialize(_stats.Definition.DeathEffectPool);
                         }
                         gameObject.SetActive(false);
                     }
                }
            }
        }
    }
}