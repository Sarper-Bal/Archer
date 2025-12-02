using UnityEngine;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D.Data; // EnemyDefinition ve Config için
using IndianOceanAssets.Engine2_5D;      // EnemyBehaviorController için

namespace IndianOceanAssets.Engine2_5D.Managers
{
    public class SmartWaveManager : MonoBehaviour
    {
        [Header("Ayarlar")]
        [SerializeField] private DirectorConfig _config;
        [SerializeField] private EnemyDatabase _enemyDatabase;

        [Header("Debug - İzleme (Salt Okunur)")]
        [Tooltip("Oyuncunun şu an oynadığı seviye. Sadece kazandıkça artar.")]
        [SerializeField] private int _currentWaveNumber = 1; 
        
        [Tooltip("Şu anki düşman satın alma bütçesi.")]
        [SerializeField] private float _currentTotalBudget;
        
        [SerializeField] private bool _isSpawningInProgress = false; // Spawner hala çalışıyor mu?
        
        // HashSet: Liste gibi ama araması ve silmesi çok daha hızlıdır (O(1)).
        private HashSet<EnemyBehaviorController> _activeEnemiesRegistry = new HashSet<EnemyBehaviorController>();

        // Spawner'ın okuyacağı liste
        public List<EnemyDefinition> NextWaveEnemies { get; private set; } = new List<EnemyDefinition>();
        
        // Aktif kuralı sakla (SwarmInterval vs. için)
        private WaveRule _currentRule;

        // [EVENT] WaveSpawner veya UI burayı dinleyebilir
        public System.Action OnWaveCompleted; 

        // --- PUBLIC API (Spawner ve Düşmanlar Burayı Kullanacak) ---

        public void InitializeGame()
        {
            if (_config != null) _currentTotalBudget = _config.StartingBudget;
            _currentWaveNumber = 1;
            _activeEnemiesRegistry.Clear();
        }

        /// <summary>
        /// Spawner, üretime başladığında bunu TRUE, bitirdiğinde FALSE yapar.
        /// </summary>
        public void SetSpawningStatus(bool isInProgress)
        {
            _isSpawningInProgress = isInProgress;
            
            // Eğer spawn bittiği an hiç düşman yoksa (bug veya boş wave), turu bitir.
            if (!isInProgress && _activeEnemiesRegistry.Count == 0)
            {
                OnWaveWon();
            }
        }

        /// <summary>
        /// Düşman sahneye çıktığında (OnEnable) kendini buraya kaydettirir.
        /// </summary>
        public void RegisterEnemy(EnemyBehaviorController enemy)
        {
            if (!_activeEnemiesRegistry.Contains(enemy))
            {
                _activeEnemiesRegistry.Add(enemy);
            }
        }

        /// <summary>
        /// Düşman öldüğünde veya havuza döndüğünde (OnDisable) kaydını sildirir.
        /// </summary>
        public void UnregisterEnemy(EnemyBehaviorController enemy)
        {
            if (_activeEnemiesRegistry.Contains(enemy))
            {
                _activeEnemiesRegistry.Remove(enemy);
                CheckWaveCompletion();
            }
        }

        private void CheckWaveCompletion()
        {
            // Eğer spawn işlemi bittiyse VE sahnede kayıtlı düşman kalmadıysa -> KAZANDIN
            if (!_isSpawningInProgress && _activeEnemiesRegistry.Count == 0)
            {
                OnWaveWon();
            }
        }

        // --- SPAWNER SORGULARI (Yeni Kural Sistemine Göre) ---

        public float GetSpawnDelay(EnemyCategory category)
        {
            if (_config == null || _currentRule.Equals(default(WaveRule))) return 1f;

            switch (category)
            {
                case EnemyCategory.Swarm: return _currentRule.SwarmInterval;
                case EnemyCategory.Rusher: return _currentRule.RusherInterval;
                case EnemyCategory.Tank: return _currentRule.TankInterval;
                default: return 1f;
            }
        }

        // --- DALGA OLUŞTURMA (GENERATE) ---

        public void GenerateNextWave()
        {
            if (_config == null || _enemyDatabase == null) return;

            NextWaveEnemies.Clear();
            _activeEnemiesRegistry.Clear(); // Yeni dalga için temizlik
            
            // [DEĞİŞİKLİK] Artık MinWinWave'e göre kural seçiyor.
            // _currentWaveNumber sadece kazandıkça arttığı için, oyuncu kaybederse
            // aynı kural (veya bir önceki kural) geçerli olmaya devam eder.
            _currentRule = _config.GetRuleForWave(_currentWaveNumber);
            
            if (_currentRule.Equals(default(WaveRule))) 
            {
                // Eğer hiç kural yoksa varsayılan basit bir kural oluştur
                _currentRule = new WaveRule { SwarmPercent = 100, SwarmInterval = 1.0f };
            }

            // Dağılım Hesapla
            float totalPercent = _currentRule.SwarmPercent + _currentRule.RusherPercent + _currentRule.TankPercent;
            if (totalPercent <= 0) totalPercent = 1;

            float swarmBudget = _currentTotalBudget * (_currentRule.SwarmPercent / totalPercent);
            float rusherBudget = _currentTotalBudget * (_currentRule.RusherPercent / totalPercent);
            float tankBudget = _currentTotalBudget * (_currentRule.TankPercent / totalPercent);

            Debug.Log($"🧮 Dalga {_currentWaveNumber} (MinWinWave: {_currentRule.MinWinWave}) Hazırlanıyor. Bütçe: {_currentTotalBudget:F0}");

            // Alışveriş Yap
            FillBudget(swarmBudget, EnemyCategory.Swarm);
            FillBudget(rusherBudget, EnemyCategory.Rusher);
            FillBudget(tankBudget, EnemyCategory.Tank);
            
            // Listeyi Karıştır (Shuffle)
            ShuffleList(NextWaveEnemies);
        }

        private void FillBudget(float budget, EnemyCategory category)
        {
            int safety = 0;
            while (budget > 0 && safety < 500)
            {
                EnemyDefinition enemy = _enemyDatabase.GetEnemyByCategory(category, budget);
                if (enemy != null)
                {
                    NextWaveEnemies.Add(enemy);
                    budget -= enemy.ThreatScore;
                }
                else break; // Bu bütçeye uygun düşman kalmadı
                safety++;
            }
        }
        
        // Fisher-Yates Shuffle
        private void ShuffleList<T>(List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        public void OnWaveWon()
        {
            Debug.Log($"🎉 WAVE {_currentWaveNumber} TAMAMLANDI! Bütçe Artıyor.");
            
            float bonus = _currentTotalBudget * _config.WinGrowthPercentage;
            _currentTotalBudget += bonus;
            
            // [ÖNEMLİ] Seviye sadece burada artar. Kaybederse artmaz.
            _currentWaveNumber++;
            
            OnWaveCompleted?.Invoke();
        }

        public void OnWaveLost()
        {
            Debug.Log($"💀 WAVE {_currentWaveNumber} KAYBEDİLDİ. Bütçe Azalıyor.");

            float penalty = _currentTotalBudget * _config.LossPenaltyPercentage;
            _currentTotalBudget -= penalty;
            
            if (_currentTotalBudget < _config.StartingBudget) 
                _currentTotalBudget = _config.StartingBudget;
                
            // Not: _currentWaveNumber'ı artırmıyoruz! Oyuncu aynı seviyeyi tekrar deneyecek.
        }
        
        // --- FAILSAFE (GÜVENLİK SİGORTASI) ---
        private void Start()
        {
            InitializeGame();
            StartCoroutine(FailsafeRoutine());
        }

        private System.Collections.IEnumerator FailsafeRoutine()
        {
            var wait = new WaitForSeconds(5f);
            while (true)
            {
                yield return wait;
                
                // Eğer spawn bitti görünüyorsa ama sistemde hala adam var görünüyorsa...
                if (!_isSpawningInProgress && _activeEnemiesRegistry.Count > 0)
                {
                    // Ölmüş veya yok olmuş objeleri temizle
                    _activeEnemiesRegistry.RemoveWhere(e => e == null || !e.gameObject.activeInHierarchy);
                    
                    // Temizlik sonrası kimse kalmadıysa bitir
                    if (_activeEnemiesRegistry.Count == 0)
                    {
                        Debug.LogWarning("🛡️ Failsafe: Takılan wave zorla bitirildi.");
                        OnWaveWon();
                    }
                }
            }
        }
    }
}