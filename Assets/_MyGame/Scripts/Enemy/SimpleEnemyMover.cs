using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Pools;

namespace IndianOceanAssets.Engine2_5D
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
        
        // --- OPTİMİZASYON: Arama Zamanlayıcısı ---
        // Düşman hedefi bulamazsa her karede değil, bu sürede bir bakar.
        private float _nextSearchTime;
        private const float SEARCH_INTERVAL = 0.5f; // Saniyede 2 kez arama (Çok ideal)

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _stats = GetComponent<EnemyStats>();

            // Fizik Optimizasyonu
            _rb.useGravity = true;
            _rb.isKinematic = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _rb.mass = 50f; 
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void OnEnable()
        {
            // Düşman havuza girip çıkarsa (Respawn) hemen bir kez ara
            _target = null;
            _nextSearchTime = Time.time; // İlk aramayı hemen yap
        }

        private void Update()
        {
            // --- KRİTİK OPTİMİZASYON BURADA ---
            // Eğer hedefim yoksa...
            if (_target == null)
            {
                // ...ve arama zamanım geldiyse ara.
                if (Time.time >= _nextSearchTime)
                {
                    FindTarget();
                    _nextSearchTime = Time.time + SEARCH_INTERVAL; // Bir sonraki arama 0.5sn sonra
                }
            }
            // Hedefim var ama ölmüşse/kapanmışsa onu bırak
            else if (!_target.gameObject.activeInHierarchy)
            {
                _target = null;
            }
        }

        private void FixedUpdate()
        {
            // Hedef yoksa fizik motorunu boşuna çalıştırma
            if (_target == null || _stats.Definition == null) return;

            MoveLogic();
        }

        private void FindTarget()
        {
            // Bu işlem "Pahalıdır" (CPU yorar), o yüzden Update içinde kısıtlı çağırıyoruz.
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
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, lookRotation, _rotationSpeed * Time.fixedDeltaTime));

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

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag(_targetTag))
            {
                ExplodeAndDie(collision.gameObject);
            }
        }

        private void ExplodeAndDie(GameObject targetObj)
        {
            if (_stats.Definition == null) return;

            float damageAmount = _stats.Definition.ContactDamage;
            
            if (targetObj.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(damageAmount);
            }
            else
            {
                var parentDamageable = targetObj.GetComponentInParent<IDamageable>();
                if (parentDamageable != null)
                {
                    parentDamageable.TakeDamage(damageAmount);
                }
            }

            if (_showDebugLogs) Debug.Log($"💥 {name} patladı!");

            if (_stats.Definition.DeathEffectPool != null)
            {
                var effect = _stats.Definition.DeathEffectPool.Get();
                effect.transform.position = transform.position;
                effect.transform.rotation = Quaternion.identity;
                effect.Initialize(_stats.Definition.DeathEffectPool);
            }

            gameObject.SetActive(false);
        }
    }
}