using UnityEngine;
using IndianOceanAssets.Engine2_5D;

namespace ArcadeBridge.ArcadeIdleEngine.Enemy
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(EnemyStats))] 
    public class SimpleEnemyMover : MonoBehaviour
    {
        [Header("Hedef Ayarları")]
        [SerializeField] private string _targetTag = "Player";
        [SerializeField] private float _rotationSpeed = 5f;

        [Header("Debug (Mobilde Kapatın)")]
        [SerializeField] private bool _showDebugLogs = false; 

        private Transform _target;
        private Rigidbody _rb;
        private EnemyStats _stats;
        
        // Arama Zamanlayıcısı
        private float _nextSearchTime;
        private const float SEARCH_INTERVAL = 0.5f; 

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _stats = GetComponent<EnemyStats>();

            // Fizik Optimizasyonu
            _rb.useGravity = true;
            _rb.isKinematic = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void OnEnable()
        {
            _target = null;
            // Yük dengeleme için rastgele başlangıç süresi
            _nextSearchTime = Time.time + Random.Range(0f, SEARCH_INTERVAL);
            
            // Hızı sıfırla
            #if UNITY_6000_0_OR_NEWER
            _rb.linearVelocity = Vector3.zero; 
            #else
            _rb.velocity = Vector3.zero;
            #endif
        }

        private void Update()
        {
            // Hedefim yoksa veya hedef öldüyse/kapandıysa
            if (_target == null || !_target.gameObject.activeInHierarchy)
            {
                // [DÜZELTME] Referans ölü ise önce onu temizle
                if (_target != null) _target = null;

                // Arama zamanı geldiyse ara
                if (Time.time >= _nextSearchTime)
                {
                    FindTarget();
                    _nextSearchTime = Time.time + SEARCH_INTERVAL; 
                }
            }
            // [DÜZELTME] Hatalı olan "_target = null" satırı buradan kaldırıldı.
        }

        private void FixedUpdate()
        {
            // Hedef yoksa hareket etme
            if (_target == null || _stats.Definition == null) return;

            MoveLogic();
        }

        private void FindTarget()
        {
            GameObject targetObj = GameObject.FindGameObjectWithTag(_targetTag);
            if (targetObj != null) 
            {
                _target = targetObj.transform;
                if (_showDebugLogs) Debug.Log($"{name}: Hedef bulundu -> {_target.name}");
            }
        }

        private void MoveLogic()
        {
            Vector3 direction = (_target.position - transform.position).normalized;
            direction.y = 0; 

            if (direction != Vector3.zero)
            {
                // Dönüş
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, lookRotation, _rotationSpeed * Time.fixedDeltaTime));

                // Hareket
                Vector3 velocity = direction * _stats.Definition.MoveSpeed;
                
                #if UNITY_6000_0_OR_NEWER
                velocity.y = _rb.linearVelocity.y; // Yerçekimini koru
                _rb.linearVelocity = velocity;
                #else
                velocity.y = _rb.velocity.y;
                _rb.velocity = velocity;
                #endif
            }
        }
        
        // Not: Hasar verme (Collision) kodları artık 'EnemyContactDamager' içinde olduğu için burada yok.
    }
}