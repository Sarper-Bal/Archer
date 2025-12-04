using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IndianOceanAssets.Engine2_5D; 
using IndianOceanAssets.Engine2_5D.Managers; 
using DG.Tweening; 

namespace ArcadeBridge.ArcadeIdleEngine.Interactables
{
    public class DestructibleBarrier : MonoBehaviour, IDamageable
    {
        [Header("Dayanıklılık")]
        [SerializeField] private float _maxHealth = 1000f;

        [Header("Görsel Parçalar")]
        [SerializeField] private GameObject _barrierModel; 
        [SerializeField] private Canvas _uiCanvas;         
        [SerializeField] private Image _fillBar;           
        [SerializeField] private TextMeshProUGUI _healthText; 

        [Header("Efektler")]
        [SerializeField] private ParticleSystem _destructionParticles; 
        
        private float _currentHealth;
        private bool _isDestroyed = false;
        private SmartWaveManager _waveManager; 

        public bool IsDead => _isDestroyed;
        public float CurrentHealth => _currentHealth;

        public event System.Action<float> OnHealthChanged;
        public event System.Action OnDeath;
        public event System.Action OnDamageTaken;

        private void Awake()
        {
            _waveManager = FindObjectOfType<SmartWaveManager>();
        }

        private void Start()
        {
            if (_waveManager == null) _waveManager = FindObjectOfType<SmartWaveManager>();

            if (_waveManager != null)
            {
                _waveManager.OnGameReset += ResetBarrier;
            }
            
            ResetBarrier();
        }

        private void OnDestroy()
        {
            if (_waveManager != null) _waveManager.OnGameReset -= ResetBarrier;
        }

        public void InitializeHealth(float maxHealth, DeathEffectPool deathPool) 
        {
            _maxHealth = maxHealth;
            ResetBarrier();
        }

        public void TakeDamage(float amount)
        {
            if (_isDestroyed) return;

            _currentHealth -= amount;
            OnDamageTaken?.Invoke();
            UpdateUI();

            if (_currentHealth <= 0)
            {
                BreakBarrier();
            }
        }

        public void Heal(float amount) { } 
        public void ResetHealth() => ResetBarrier();

        private void BreakBarrier()
        {
            _isDestroyed = true;
            
            // 1. Ölüm haberini yay (Bu, BaseObjectiveController'ı tetikleyip Reset'i başlatabilir)
            OnDeath?.Invoke();

            // [KRİTİK DÜZELTME] Haber verdikten sonra kontrol et:
            // Eğer OnDeath zinciri sırasında oyun resetlendiyse, ben artık "Destroyed" değilimdir.
            // O yüzden aşağıdaki kapatma işlemlerini İPTAL ET.
            if (!_isDestroyed) 
            {
                // Debug.Log("🛡️ Yıkılma iptal edildi çünkü reset geldi.");
                return; 
            }

            // Eğer reset gelmediyse normal yıkılma işlemine devam et
            if (_barrierModel) _barrierModel.SetActive(false);
            if (_uiCanvas) _uiCanvas.gameObject.SetActive(false);
            if (_destructionParticles) _destructionParticles.Play();

            var navObstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();
            if (navObstacle) navObstacle.enabled = false;
            
            var col = GetComponent<Collider>();
            if (col) col.enabled = false;
        }

        private void ResetBarrier()
        {
            _currentHealth = _maxHealth;
            
            // [ÖNEMLİ] Burası false yapıldığı için yukarıdaki BreakBarrier duracak
            _isDestroyed = false; 

            // 1. Görselleri Aç ve Düzelt
            if (_barrierModel) 
            {
                _barrierModel.SetActive(true);
                _barrierModel.transform.DOKill(); 
                _barrierModel.transform.localScale = Vector3.one; 
            }

            if (_uiCanvas) 
            {
                _uiCanvas.gameObject.SetActive(true);
                _uiCanvas.transform.DOKill();
                _uiCanvas.transform.localScale = Vector3.one;
            }
            
            var navObstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();
            if (navObstacle) navObstacle.enabled = true;

            var col = GetComponent<Collider>();
            if (col) col.enabled = true;

            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_healthText != null) _healthText.text = Mathf.Max(0, _currentHealth).ToString("F0");
            if (_fillBar != null) _fillBar.fillAmount = _currentHealth / _maxHealth;
        }
    }
}