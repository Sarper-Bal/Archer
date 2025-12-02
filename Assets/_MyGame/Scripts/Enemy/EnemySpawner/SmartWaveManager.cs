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
        [SerializeField] private int _currentWaveNumber = 1;
        [SerializeField] private float _currentTotalBudget;
        [SerializeField] private bool _isSpawningInProgress = false; // Spawner hala çalışıyor mu?
        
        // HashSet: Liste gibi ama araması ve silmesi çok daha hızlıdır (O(1)).
        // Ayrıca aynı düşmanı yanlışlıkla 2 kere eklemenizi engeller.
        private HashSet<EnemyBehaviorController> _activeEnemiesRegistry = new HashSet<EnemyBehaviorController>();

        // Spawner'ın okuyacağı liste
        public List<EnemyDefinition> NextWaveEnemies { get; private set; } = new List<EnemyDefinition>();
        
        private WaveRule _currentRule;

        // --- PUBLIC API (Spawner ve Düşmanlar Burayı Kullanacak) ---

        public void InitializeGame()
        {
            if (_config != null) _currentTotalBudget = _config.StartingBudget;
            _activeEnemiesRegistry.Clear();
        }

        /// <summary>
        /// Spawner, üretime başladığında bunu TRUE, bitirdiğinde FALSE yapar.
        /// Bu sayede ilk düşman öldüğünde "Wave Bitti" sanmasını engelleriz.
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
                
                // Kalan düşman sayısını kontrol et
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

        // --- EKONOMİ VE DALGA OLUŞTURMA ---

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

        public void GenerateNextWave()
        {
            if (_config == null || _enemyDatabase == null) return;

            NextWaveEnemies.Clear();
            _activeEnemiesRegistry.Clear(); // Yeni dalga için temizlik
            
            _currentRule = _config.GetRuleForWave(_currentWaveNumber);
            
            if (_currentRule.Equals(default(WaveRule))) 
            {
                _currentRule = new WaveRule { SwarmPercent = 100, SwarmInterval = 0.5f };
            }

            float totalPercent = _currentRule.SwarmPercent + _currentRule.RusherPercent + _currentRule.TankPercent;
            if (totalPercent <= 0) totalPercent = 1;

            float swarmBudget = _currentTotalBudget * (_currentRule.SwarmPercent / totalPercent);
            float rusherBudget = _currentTotalBudget * (_currentRule.RusherPercent / totalPercent);
            float tankBudget = _currentTotalBudget * (_currentRule.TankPercent / totalPercent);

            Debug.Log($"🧮 Dalga {_currentWaveNumber} Hazırlanıyor. Bütçe: {_currentTotalBudget}");

            FillBudget(swarmBudget, EnemyCategory.Swarm);
            FillBudget(rusherBudget, EnemyCategory.Rusher);
            FillBudget(tankBudget, EnemyCategory.Tank);
            
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
                else break;
                safety++;
            }
        }
        
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

        // [EVENT] WaveSpawner burayı dinleyebilir (Action eklenebilir)
        public System.Action OnWaveCompleted; 

        public void OnWaveWon()
        {
            Debug.Log($"🎉 WAVE {_currentWaveNumber} TAMAMLANDI! (Tüm düşmanlar temizlendi)");
            
            float bonus = _currentTotalBudget * _config.WinGrowthPercentage;
            _currentTotalBudget += bonus;
            _currentWaveNumber++;
            
            // Spawner'a "Ben bittim" sinyali gönder
            OnWaveCompleted?.Invoke();
        }

        public void OnWaveLost()
        {
            float penalty = _currentTotalBudget * _config.LossPenaltyPercentage;
            _currentTotalBudget -= penalty;
            if (_currentTotalBudget < _config.StartingBudget) _currentTotalBudget = _config.StartingBudget;
        }
        
        // --- FAILSAFE (GÜVENLİK SİGORTASI) ---
        // Eğer bir şekilde sayı takılı kalırsa diye her 5 saniyede bir çalışır
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
                    // Registry'deki elemanları kontrol et, ölmüş veya null olanları temizle
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