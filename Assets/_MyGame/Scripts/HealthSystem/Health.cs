using UnityEngine;
using System;
using ArcadeBridge.ArcadeIdleEngine.Pools; // Eğer namespace farklıysa düzelt

namespace IndianOceanAssets.Engine2_5D
{
    public class Health : MonoBehaviour, IDamageable
    {
        [Header("Varsayılan Ayarlar")]
        [Tooltip("Eğer dışarıdan bir veri dosyası atanmazsa bu değer kullanılır.")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private bool _isInvincible = false;

        [Header("Görsel Efektler")]
        [SerializeField] private DeathEffectPool _defaultDeathEffectPool;
        
        private float _currentHealth;
        private bool _isDead;
        private DeathEffectPool _currentDeathPool; // O anki aktif havuz

        public event Action<float> OnHealthChanged;
        public event Action OnDeath;
        public event Action OnDamageTaken;

        public bool IsDead => _isDead;
        public float CurrentHealth => _currentHealth;

        private void Awake()
        {
            // Başlangıçta varsayılan efekti kullan
            _currentDeathPool = _defaultDeathEffectPool;
        }

        private void OnEnable()
        {
            // Obje havuza girip çıktığında canı resetle
            ResetHealth();
        }

        // --- YENİ EKLENEN KISIM: DIŞARIDAN VERİ GİRİŞİ ---
        /// <summary>
        /// Silah veya Düşman verisinden gelen değerlerle Can sistemini kurar.
        /// </summary>
        public void InitializeHealth(float newMaxHealth, DeathEffectPool deathPool = null)
        {
            _maxHealth = newMaxHealth; // Max canı güncelle
            
            if (deathPool != null)
            {
                _currentDeathPool = deathPool; // Düşmanın özel ölüm efekti varsa onu kullan
            }

            ResetHealth(); // Canı yeni değerle fulle
        }
        // -------------------------------------------------

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
            // Artık _currentDeathPool kullanıyoruz
            if (_currentDeathPool != null)
            {
                var effect = _currentDeathPool.Get();
                effect.transform.position = transform.position + Vector3.up;
                effect.transform.rotation = Quaternion.identity;
                effect.Initialize(_currentDeathPool);
            }
        }
    }
}