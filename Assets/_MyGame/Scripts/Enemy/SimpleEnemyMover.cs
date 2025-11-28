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

        [Header("Debug")]
        [Tooltip("İşaretlenirse konsola detaylı bilgiler yazar (Mobilde kapatın)")]
        [SerializeField] private bool _showDebugLogs = false;

        private Transform _target;
        private Rigidbody _rb;
        private EnemyStats _stats;
        
        // OPTİMİZASYON 1: Arama Zamanlayıcısı
        private float _nextSearchTime;
        private const float SEARCH_INTERVAL = 0.5f; // Hedefi bulamazsa 0.5 saniyede bir ara

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _stats = GetComponent<EnemyStats>();

            _rb.useGravity = true;
            _rb.isKinematic = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _rb.mass = 50f; 
            // Interpolate, hareketi yumuşatır (daha az titreme)
            _rb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void OnEnable()
        {
            // Doğduğunda hemen bir kere ara
            FindTarget();
            _nextSearchTime = Time.time + SEARCH_INTERVAL;
        }

        private void Update()
        {
            // OPTİMİZASYON: Her karede değil, sadece zamanı gelince ara
            if (_target == null)
            {
                if (Time.time >= _nextSearchTime)
                {
                    FindTarget();
                    _nextSearchTime = Time.time + SEARCH_INTERVAL;
                }
            }
            else if (!_target.gameObject.activeInHierarchy)
            {
                // Hedef var ama pasif (ölü) ise hedefi bırak
                _target = null;
            }
        }

        private void FixedUpdate()
        {
            // Hedef yoksa veya Veri yüklenmediyse fizik motorunu yorma
            if (_target == null || _stats.Definition == null) return;

            MoveLogic();
        }

        private void FindTarget()
        {
            GameObject targetObj = GameObject.FindGameObjectWithTag(_targetTag);
            if (targetObj != null) _target = targetObj.transform;
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
                
                // Yerçekimi Koruması (Unity Versiyon Kontrolü)
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
            // Tag kontrolü en hızlı (GC-Free) kontroldür
            if (collision.gameObject.CompareTag(_targetTag))
            {
                ExplodeAndDie(collision.gameObject);
            }
        }

        private void ExplodeAndDie(GameObject targetObj)
        {
            if (_stats.Definition == null) return;

            float damageAmount = _stats.Definition.ContactDamage;
            
            // 1. Hasar Ver
            bool damageDealt = false;
            if (targetObj.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(damageAmount);
                damageDealt = true;
            }
            else
            {
                // Collider çocuk objede olabilir, parent'ı kontrol et
                var parentDamageable = targetObj.GetComponentInParent<IDamageable>();
                if (parentDamageable != null)
                {
                    parentDamageable.TakeDamage(damageAmount);
                    damageDealt = true;
                }
            }

            // 2. Debug Log (Sadece Geliştirme Modunda)
            // Bu string birleştirme işlemi mobilde bellek (GC) üretir, o yüzden şarta bağladık.
            if (_showDebugLogs && damageDealt)
            {
                 Debug.Log($"<color=red>💥 KAMIKAZE!</color> {gameObject.name} patladı ve {damageAmount} hasar verdi.");
            }

            // 3. Efekt Oynat (Havuzdan)
            PlayDeathEffect();

            // 4. Yok Ol
            gameObject.SetActive(false);
        }

        private void PlayDeathEffect()
        {
            if (_stats.Definition.DeathEffectPool != null)
            {
                var deathPool = _stats.Definition.DeathEffectPool;
                var effect = deathPool.Get();
                
                effect.transform.position = transform.position;
                effect.transform.rotation = Quaternion.identity;
                
                effect.Initialize(deathPool);
            }
        }
    }
}