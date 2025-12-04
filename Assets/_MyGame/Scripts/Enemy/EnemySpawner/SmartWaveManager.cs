using UnityEngine;
using System.Collections; 
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D.Data;
using IndianOceanAssets.Engine2_5D;

namespace IndianOceanAssets.Engine2_5D.Managers
{
    public class SmartWaveManager : MonoBehaviour
    {
        [Header("Ayarlar")]
        [SerializeField] private DirectorConfig _config;
        [SerializeField] private EnemyDatabase _enemyDatabase;

        [Header("Debug - İzleme")]
        [SerializeField] private int _currentWaveNumber = 1;
        [SerializeField] private float _currentTotalBudget;
        [SerializeField] private bool _isSpawningInProgress = false;
        
        // [YENİ] Reset işlemi sırasında "Kazandın" kontrolünü engellemek için bayrak
        private bool _isResetting = false; 
        
        private HashSet<EnemyBehaviorController> _activeEnemiesRegistry = new HashSet<EnemyBehaviorController>();

        public List<EnemyDefinition> NextWaveEnemies { get; private set; } = new List<EnemyDefinition>();
        private WaveRule _currentRule;

        public event System.Action OnWaveCompleted; 
        public event System.Action OnGameReset;     

        private void Start()
        {
            InitializeGame();
            StartCoroutine(FailsafeRoutine());
        }

        public void InitializeGame()
        {
            if (_config != null) _currentTotalBudget = _config.StartingBudget;
            _currentWaveNumber = 1;
            _activeEnemiesRegistry.Clear();
            _isResetting = false;
        }

        // --- KAYBETME & RESET MANTIĞI (DÜZELTİLDİ) ---

        public void TriggerWaveFailure()
        {
            // Eğer zaten resetleniyorsa veya spawn bitmiş ve düşman yoksa (hatalı çağrı) çık
            if (_isResetting) return;

            Debug.Log("❌ WAVE BAŞARISIZ! Sistem Resetleniyor...");

            // 1. Reset Modunu Aç (Kritik: Bu sayede düşmanlar silinirken 'Kazandın' tetiklenmez)
            _isResetting = true;
            _isSpawningInProgress = false;

            // 2. Düşmanları Temizle
            var enemiesToClear = new List<EnemyBehaviorController>(_activeEnemiesRegistry);
            foreach (var enemy in enemiesToClear)
            {
                if (enemy != null) enemy.gameObject.SetActive(false); 
            }
            _activeEnemiesRegistry.Clear();

            // 3. Cezalandır ve Tamir Et
            OnWaveLost();
            OnGameReset?.Invoke();

            // 4. Reset Modunu Kapat
            _isResetting = false;

            // 5. Spawner'a "Sıradaki Wave'e Hazırlan" De
            // (Burada ekstra süre beklemiyoruz, Spawner kendi süresini sayacak)
            Debug.Log("🔄 Wave Tekrarı İçin Sinyal Gönderiliyor...");
            OnWaveCompleted?.Invoke();
        }

        // --- DİĞER MANTIKLAR ---

        public void SetSpawningStatus(bool isInProgress)
        {
            _isSpawningInProgress = isInProgress;
            CheckWaveCompletion();
        }

        public void RegisterEnemy(EnemyBehaviorController enemy)
        {
            if (!_activeEnemiesRegistry.Contains(enemy)) _activeEnemiesRegistry.Add(enemy);
        }

        public void UnregisterEnemy(EnemyBehaviorController enemy)
        {
            if (_activeEnemiesRegistry.Contains(enemy))
            {
                _activeEnemiesRegistry.Remove(enemy);
                
                // [DÜZELTME] Eğer reset atıyorsak, düşman azaldı diye kontrol yapma
                if (!_isResetting)
                {
                    CheckWaveCompletion();
                }
            }
        }

        private void CheckWaveCompletion()
        {
            // Eğer reset modundaysak asla kazanma kontrolü yapma
            if (_isResetting) return;

            if (!_isSpawningInProgress && _activeEnemiesRegistry.Count == 0)
            {
                OnWaveWon();
            }
        }

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
            _activeEnemiesRegistry.Clear();
            
            _currentRule = _config.GetRuleForWave(_currentWaveNumber);
            
            if (_currentRule.Equals(default(WaveRule))) 
                _currentRule = new WaveRule { SwarmPercent = 100, SwarmInterval = 1.0f };

            float totalPercent = _currentRule.SwarmPercent + _currentRule.RusherPercent + _currentRule.TankPercent;
            if (totalPercent <= 0) totalPercent = 1;

            float swarmBudget = _currentTotalBudget * (_currentRule.SwarmPercent / totalPercent);
            float rusherBudget = _currentTotalBudget * (_currentRule.RusherPercent / totalPercent);
            float tankBudget = _currentTotalBudget * (_currentRule.TankPercent / totalPercent);

            Debug.Log($"🧮 Dalga {_currentWaveNumber} Hazırlanıyor. Bütçe: {_currentTotalBudget:F0}");

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

        public void OnWaveWon()
        {
            Debug.Log($"🎉 WAVE {_currentWaveNumber} KAZANILDI!");
            float bonus = _currentTotalBudget * _config.WinGrowthPercentage;
            _currentTotalBudget += bonus;
            _currentWaveNumber++;
            OnWaveCompleted?.Invoke();
        }

        public void OnWaveLost()
        {
            Debug.Log($"💀 WAVE KAYBEDİLDİ. Bütçe Düşürülüyor.");
            float penalty = _currentTotalBudget * _config.LossPenaltyPercentage;
            _currentTotalBudget -= penalty;
            if (_currentTotalBudget < _config.StartingBudget) _currentTotalBudget = _config.StartingBudget;
            
            // [ÖNEMLİ] Wave numarasını düşürmüyoruz, oyuncu aynı seviyeyi (kolaylaşmış halde) tekrar deneyecek.
        }
        
        private IEnumerator FailsafeRoutine()
        {
            var wait = new WaitForSeconds(5f);
            while (true)
            {
                yield return wait;
                // Reset sırasında failsafe çalışmasın
                if (!_isResetting && !_isSpawningInProgress && _activeEnemiesRegistry.Count > 0)
                {
                    _activeEnemiesRegistry.RemoveWhere(e => e == null || !e.gameObject.activeInHierarchy);
                    if (_activeEnemiesRegistry.Count == 0)
                    {
                        Debug.LogWarning("🛡️ Failsafe: Takılan wave temizlendi.");
                        OnWaveWon();
                    }
                }
            }
        }
    }
}