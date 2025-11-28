using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Pools; // Efekt havuzu için gerekli

namespace IndianOceanAssets.Engine2_5D
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(EnemyStats))] 
    public class SimpleEnemyMover : MonoBehaviour
    {
        [Header("Hedef Ayarları")]
        [SerializeField] private string _targetTag = "Player";
        [SerializeField] private float _rotationSpeed = 5f;

        private Transform _target;
        private Rigidbody _rb;
        private EnemyStats _stats;
        
        // Hareket için FixedUpdate kullanıyoruz, o yüzden değişkenleri cache'liyoruz
        private Vector3 _currentDirection;
        private float _currentSpeed;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _stats = GetComponent<EnemyStats>();

            _rb.useGravity = true;
            _rb.isKinematic = false;
            _rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _rb.mass = 50f; 
        }

        private void OnEnable()
        {
            FindTarget();
        }

        private void Update()
        {
            if (_target == null) FindTarget();
        }

        private void FixedUpdate()
        {
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
                
                // Yerçekimini koru
#if UNITY_6000_0_OR_NEWER
                velocity.y = _rb.linearVelocity.y;
                _rb.linearVelocity = velocity;
#else
                velocity.y = _rb.velocity.y;
                _rb.velocity = velocity;
#endif
            }
        }

        // --- ÇARPIŞMA VE PATLAMA MANTIĞI ---
        private void OnCollisionEnter(Collision collision)
        {
            // Sadece Player'a çarpınca patla
            if (collision.gameObject.CompareTag(_targetTag))
            {
                ExplodeAndDie(collision.gameObject);
            }
        }

        private void ExplodeAndDie(GameObject targetObj)
        {
            if (_stats.Definition == null) return;

            float damageAmount = _stats.Definition.ContactDamage;
            float playerHealthLeft = -1f; // Varsayılan

            // 1. Oyuncuya Hasar Ver
            if (targetObj.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(damageAmount);

                // Log için oyuncunun can scriptine ulaşmaya çalış
                if (targetObj.TryGetComponent(out Health playerHealth))
                {
                    playerHealthLeft = playerHealth.CurrentHealth;
                }
            }

            // 2. Konsola Detaylı Yaz
            Debug.Log($"<color=red>💥 PATLAMA!</color> Düşman kendini feda etti.\n" +
                      $"⚔️ Verilen Hasar: <b>{damageAmount}</b>\n" +
                      $"❤️ Player Kalan Can: <b>{playerHealthLeft}</b>");

            // 3. Ölüm Efektini Çıkar (Varsa)
            PlayDeathEffect();

            // 4. Düşmanı Yok Et (Havuza Gönder veya Kapat)
            gameObject.SetActive(false);
        }

        private void PlayDeathEffect()
        {
            // EnemyStats üzerinden Definition'a, oradan da Pool'a ulaşıyoruz
            if (_stats.Definition.DeathEffectPool != null)
            {
                var deathPool = _stats.Definition.DeathEffectPool;
                var effect = deathPool.Get();
                
                effect.transform.position = transform.position;
                effect.transform.rotation = Quaternion.identity;
                
                // Efekti başlat (Kendi süresi bitince havuza döner)
                effect.Initialize(deathPool);
            }
        }
    }
}