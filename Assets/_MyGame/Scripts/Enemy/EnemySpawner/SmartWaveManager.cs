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

        [Header("Debug")]
        [SerializeField] private int _currentWaveNumber = 1;
        [SerializeField] private float _currentTotalBudget;
        [SerializeField] private bool _isSpawningInProgress = false;
        private bool _isResetting = false; 
        
        private HashSet<EnemyBehaviorController> _activeEnemiesRegistry = new HashSet<EnemyBehaviorController>();
        public List<EnemyDefinition> NextWaveEnemies { get; private set; } = new List<EnemyDefinition>();
        private WaveRule _currentRule;

        // --- EVENTLER (Eksik olan eklendi) ---
        public event System.Action OnWaveStarted;   // [YENİ] Savaş başladı sinyali
        public event System.Action OnWaveCompleted; // Savaş bitti (Kazanma)
        public event System.Action OnGameReset;     // Oyun resetlendi (Kaybetme/Tamir)

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

        // --- YENİ: Spawner bu fonksiyonu çağırarak savaşı başlattığını haber verir ---
        public void NotifyWaveStarted()
        {
            OnWaveStarted?.Invoke();
        }

        // --- KAYBETME & RESET ---
        public void TriggerWaveFailure()
        {
            if (_isResetting) return;

            Debug.Log("❌ WAVE BAŞARISIZ! Resetleniyor...");
            _isResetting = true;
            _isSpawningInProgress = false;

            var enemiesToClear = new List<EnemyBehaviorController>(_activeEnemiesRegistry);
            foreach (var enemy in enemiesToClear)
            {
                if (enemy != null) enemy.gameObject.SetActive(false); 
            }
            _activeEnemiesRegistry.Clear();

            OnWaveLost();
            OnGameReset?.Invoke(); // Kapılar ve Upgrade kutuları burada resetlenir

            _isResetting = false;
            Debug.Log("🔄 Yeni Wave İsteniyor...");
            OnWaveCompleted?.Invoke();
        }

        // --- STANDART MANTIKLAR ---
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
                if (!_isResetting) CheckWaveCompletion();
            }
        }

        private void CheckWaveCompletion()
        {
            if (_isResetting) return;
            if (!_isSpawningInProgress && _activeEnemiesRegistry.Count == 0) OnWaveWon();
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
            if (_currentRule.Equals(default(WaveRule))) _currentRule = new WaveRule { SwarmPercent = 100, SwarmInterval = 1.0f };

            float totalPercent = _currentRule.SwarmPercent + _currentRule.RusherPercent + _currentRule.TankPercent;
            if (totalPercent <= 0) totalPercent = 1;

            float swarmBudget = _currentTotalBudget * (_currentRule.SwarmPercent / totalPercent);
            float rusherBudget = _currentTotalBudget * (_currentRule.RusherPercent / totalPercent);
            float tankBudget = _currentTotalBudget * (_currentRule.TankPercent / totalPercent);

            FillBudget(swarmBudget, EnemyCategory.Swarm);
            FillBudget(rusherBudget, EnemyCategory.Rusher);
            FillBudget(tankBudget, EnemyCategory.Tank);
            ShuffleList(NextWaveEnemies);
            
            Debug.Log($"🧮 Wave {_currentWaveNumber} Hazırlandı. Bütçe: {_currentTotalBudget:F0}");
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

            OnGameReset?.Invoke(); // Kazanılınca da yapıları tamir et
            OnWaveCompleted?.Invoke();
        }

        public void OnWaveLost()
        {
            float penalty = _currentTotalBudget * _config.LossPenaltyPercentage;
            _currentTotalBudget -= penalty;
            if (_currentTotalBudget < _config.StartingBudget) _currentTotalBudget = _config.StartingBudget;
        }
        
        private IEnumerator FailsafeRoutine()
        {
            var wait = new WaitForSeconds(5f);
            while (true)
            {
                yield return wait;
                if (!_isResetting && !_isSpawningInProgress && _activeEnemiesRegistry.Count > 0)
                {
                    _activeEnemiesRegistry.RemoveWhere(e => e == null || !e.gameObject.activeInHierarchy);
                    if (_activeEnemiesRegistry.Count == 0)
                    {
                        Debug.LogWarning("🛡️ Failsafe: Temizlik yapıldı.");
                        OnWaveWon();
                    }
                }
            }
        }
    }
}