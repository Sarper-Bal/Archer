using UnityEngine;
using System;
using ArcadeBridge.ArcadeIdleEngine.Pools;
using DG.Tweening; // [EKLENDI] Animasyon kütüphanesi eklendi.

namespace IndianOceanAssets.Engine2_5D
{
    public class Health : MonoBehaviour, IDamageable
    {
        [Header("Default Settings / Varsayılan Ayarlar")]
        [Tooltip("If no data is assigned externally, this value is used. / Eğer dışarıdan bir veri dosyası atanmazsa bu değer kullanılır.")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private bool _isInvincible = false;

        [Header("Visual Effects / Görsel Efektler")]
        [SerializeField] private DeathEffectPool _defaultDeathEffectPool;
        
        // [EKLENDI] Hasar animasyonu için modelin transform referansı (Genelde child objedir ama burada kendisi varsayıyoruz)
        [SerializeField] private Transform _modelTransform; 

        private float _currentHealth;
        private bool _isDead;
        private DeathEffectPool _currentDeathPool; // Current active pool / O anki aktif havuz

        public event Action<float> OnHealthChanged;
        public event Action OnDeath;
        public event Action OnDamageTaken;

        public bool IsDead => _isDead;
        public float CurrentHealth => _currentHealth;

        private void Awake()
        {
            // Use default effect initially / Başlangıçta varsayılan efekti kullan
            _currentDeathPool = _defaultDeathEffectPool;

            // [EKLENDI] Eğer model transform atanmamışsa, bu objenin kendisini al.
            if (_modelTransform == null) _modelTransform = transform;
        }

        private void OnEnable()
        {
            // Reset health when pooling / Obje havuza girip çıktığında canı resetle
            ResetHealth();
            
            // [EKLENDI] Olası bir scale bozulmasına karşı scale'i düzelt
            _modelTransform.localScale = Vector3.one; 
        }

        private void OnDisable()
        {
            // [EKLENDI] Obje kapanırken üzerindeki tüm tween'leri öldür (Hata almamak için)
            _modelTransform.DOKill();
        }

        /// <summary>
        /// Sets up the Health system with data from Weapon or Enemy data.
        /// Silah veya Düşman verisinden gelen değerlerle Can sistemini kurar.
        /// </summary>
        public void InitializeHealth(float newMaxHealth, DeathEffectPool deathPool = null)
        {
            _maxHealth = newMaxHealth; // Update Max Health / Max canı güncelle
            
            if (deathPool != null)
            {
                _currentDeathPool = deathPool; // Use custom death effect if available / Düşmanın özel ölüm efekti varsa onu kullan
            }

            ResetHealth(); // Full heal with new value / Canı yeni değerle fulle
        }

        public void ResetHealth()
        {
            _currentHealth = _maxHealth;
            _isDead = false;
            OnHealthChanged?.Invoke(1f);
        }

        public void TakeDamage(float amount)
        {
            if (_isDead || _isInvincible) return;

            _currentHealth -= amount;
            OnDamageTaken?.Invoke();
            OnHealthChanged?.Invoke(_currentHealth / _maxHealth);

            // [EKLENDI] Hasar Geri Bildirimi (Juice)
            PlayHitFeedback();

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(float amount)
        {
            if (_isDead) return;
            _currentHealth += amount;
            if (_currentHealth > _maxHealth) _currentHealth = _maxHealth;
            OnHealthChanged?.Invoke(_currentHealth / _maxHealth);
        }

        private void Die()
        {
            if (_isDead) return;
            _isDead = true;
            OnDeath?.Invoke();
            PlayDeathEffect();

            if (gameObject.CompareTag("Player"))
            {
                Debug.Log("GAME OVER");
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void PlayDeathEffect()
        {
            // We use _currentDeathPool now / Artık _currentDeathPool kullanıyoruz
            if (_currentDeathPool != null)
            {
                var effect = _currentDeathPool.Get();
                effect.transform.position = transform.position + Vector3.up;
                effect.transform.rotation = Quaternion.identity;
                effect.Initialize(_currentDeathPool);
            }
        }

        // [EKLENDI] Cartoon tarzı vurulma efekti
        private void PlayHitFeedback()
        {
            // Eğer zaten bir animasyon varsa onu durdur ve baştan başlat (rewind)
            _modelTransform.DOKill(true);
            
            // Objeyi hafifçe sıkıştırıp (Punch) sallıyoruz. 
            // 0.2f süre, 0.1f şiddet (azaltılabilir), 10 vibrasyon.
            _modelTransform.DOPunchScale(Vector3.one * -0.2f, 0.2f, 10, 1f).SetEase(Ease.OutQuad);
        }
    }
}