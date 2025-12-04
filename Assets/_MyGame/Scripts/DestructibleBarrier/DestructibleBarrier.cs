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

        // [YENİ] Orijinal Boyutları Saklamak İçin
        private Vector3 _originalModelScale;
        private Vector3 _originalCanvasScale;

        public bool IsDead => _isDestroyed;
        public float CurrentHealth => _currentHealth;

        public event System.Action<float> OnHealthChanged;
        public event System.Action OnDeath;
        public event System.Action OnDamageTaken;

        private void Awake()
        {
            _waveManager = FindObjectOfType<SmartWaveManager>();

            // [KRİTİK] Oyun başlar başlamaz, senin ayarladığın boyutu hafızaya al
            if (_barrierModel != null) _originalModelScale = _barrierModel.transform.localScale;
            if (_uiCanvas != null) _originalCanvasScale = _uiCanvas.transform.localScale;
        }

        private void Start()
        {
            if (_waveManager == null) _waveManager = FindObjectOfType<SmartWaveManager>();

            if (_waveManager != null)
            {
                _waveManager.OnGameReset += ResetBarrier;
            }
            
            // ResetBarrier yerine, ilk başlangıçta sadece değerleri sıfırla
            // (Çünkü ResetBarrier boyutu değiştirebilir, Start'ta buna gerek yok)
            _currentHealth = _maxHealth;
            UpdateUI();
        }

        private void OnDestroy()
        {
            if (_waveManager != null) _waveManager.OnGameReset -= ResetBarrier;
        }

        public void InitializeHealth(float maxHealth, DeathEffectPool deathPool) 
        {
            _maxHealth = maxHealth;
            _currentHealth = _maxHealth;
            _isDestroyed = false;
            UpdateUI();
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
            OnDeath?.Invoke();

            if (!_isDestroyed) return; 

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
            _isDestroyed = false;

            // 1. Modeli Aç ve Orijinal Boyutuna Getir
            if (_barrierModel) 
            {
                _barrierModel.SetActive(true);
                _barrierModel.transform.DOKill(); 
                
                // [DÜZELTME] Vector3.one yerine orijinal scale kullan
                _barrierModel.transform.localScale = _originalModelScale; 
            }

            // 2. UI'ı Aç ve Orijinal Boyutuna Getir
            if (_uiCanvas) 
            {
                _uiCanvas.gameObject.SetActive(true);
                _uiCanvas.transform.DOKill();
                // [DÜZELTME] Vector3.one yerine orijinal scale kullan
                _uiCanvas.transform.localScale = _originalCanvasScale;
            }
            
            var navObstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();
            if (navObstacle) navObstacle.enabled = true;

            var col = GetComponent<Collider>();
            if (col) col.enabled = true;

            UpdateUI();
            
            Debug.Log($"♻️ {gameObject.name}: Orijinal boyutlarıyla ({_originalModelScale}) tamir edildi.");
        }

        private void UpdateUI()
        {
            if (_healthText != null) _healthText.text = Mathf.Max(0, _currentHealth).ToString("F0");
            if (_fillBar != null) _fillBar.fillAmount = _currentHealth / _maxHealth;
        }
    }
}