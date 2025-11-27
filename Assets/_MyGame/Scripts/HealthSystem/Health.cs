using UnityEngine;
using System; // Action eventleri için

namespace IndianOceanAssets.Engine2_5D
{
    public class Health : MonoBehaviour, IDamageable
    {
        [Header("Can Ayarları")]
        [SerializeField] private float _maxHealth = 100f;
        [SerializeField] private bool _isInvincible = false; // Ölümsüzlük (God Mode)

        [Header("Görsel Efektler")]
        [Tooltip("Bu karakter ölünce çalışacak efekt havuzu")]
        [SerializeField] private DeathEffectPool _deathEffectPool;
        
        // Private Değişkenler
        private float _currentHealth;
        private bool _isDead;

        // --- Eventler (Diğer sistemlerin dinlemesi için) ---
        public event Action<float> OnHealthChanged; // (Kalan Can / Max Can) oranını gönderir (Health Bar için)
        public event Action OnDeath;                // Öldüğünde tetiklenir
        public event Action OnDamageTaken;          // Hasar alındığında tetiklenir (Kızarma efekti vb. için)

        public bool IsDead => _isDead;
        public float CurrentHealth => _currentHealth;

        private void OnEnable()
        {
            ResetHealth();
        }

        /// <summary>
        /// Karakter yeniden doğduğunda veya havuzdan çıktığında canını fuller.
        /// </summary>
        public void ResetHealth()
        {
            _currentHealth = _maxHealth;
            _isDead = false;
            
            // UI güncellemesi gönder (Can full)
            OnHealthChanged?.Invoke(1f); 
        }

        // --- IDamageable Arayüzünden Gelen Metot ---
        public void TakeDamage(float amount)
        {
            if (_isDead || _isInvincible) return;

            _currentHealth -= amount;
            
            // Eventleri tetikle
            OnDamageTaken?.Invoke();
            OnHealthChanged?.Invoke(_currentHealth / _maxHealth); // 0 ile 1 arası değer yollar

            // Debug için log (İstersen kapatabilirsin)
            // Debug.Log($"{gameObject.name} hasar aldı. Kalan Can: {_currentHealth}");

            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// Can iyileştirme fonksiyonu (Potion vb. için)
        /// </summary>
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

            // 1. Ölüm Efektini Oynat (Pool Kullanarak)
            PlayDeathEffect();

            // 2. Nesneyi Sahneden Kaldır
            // Eğer bu bir oyuncuysa nesneyi kapatmak yerine "Game Over" ekranını çağırmalıyız.
            if (gameObject.CompareTag("Player"))
            {
                Debug.Log("<color=red>OYUNCU ÖLDÜ! GAME OVER.</color>");
                // Buraya LevelManager.GameOver() gibi bir kod gelecek.
                // Şimdilik sadece render'ı kapatabiliriz veya animasyon oynatabiliriz.
            }
            else
            {
                // Düşmansa kendini kapatsın (Pool sistemi onu geri almış olur)
                gameObject.SetActive(false);
            }
        }

        private void PlayDeathEffect()
        {
            if (_deathEffectPool != null)
            {
                // Havuzdan efekt çek
                var effect = _deathEffectPool.Get();
                
                // Pozisyonu ayarla (Biraz yukarıdan çıksın ki gövde ortalansın)
                effect.transform.position = transform.position + Vector3.up * 1f;
                effect.transform.rotation = Quaternion.identity;

                // Efekti başlat (ExplosionEffect scripti süresi bitince havuza geri yollar)
                effect.Initialize(_deathEffectPool); // Dikkat: ExplosionEffect parametre olarak ExplosionPool ister.
                // UYARI: DeathEffectPool sınıfı ExplosionPool ile aynı T tipinde olmalı veya
                // ExplosionEffect scriptini generic yapmalıyız. 
                // PRATİK ÇÖZÜM: Aşağıdaki notu oku.
            }
        }
    }
}