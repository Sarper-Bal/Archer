using UnityEngine;
using System.Collections; 
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D.Data;
using IndianOceanAssets.Engine2_5D;
using ArcadeBridge.ArcadeIdleEngine.Data.Variables; // Save Sistemi (Int/Float Variable)

namespace IndianOceanAssets.Engine2_5D.Managers
{
    public class SmartWaveManager : MonoBehaviour
    {
        [Header("⚙️ Ayarlar")]
        [SerializeField] private DirectorConfig _config;
        [SerializeField] private EnemyDatabase _enemyDatabase;

        [Header("💾 Kayıt Sistemi (Save)")]
        [Tooltip("Kaçıncı dalgada olduğunu tutan kayıt dosyası.")]
        [SerializeField] private IntVariable _savedWaveNumber;
        
        [Tooltip("Düşman bütçesini tutan kayıt dosyası.")]
        [SerializeField] private FloatVariable _savedBudget;

        [Header("📊 Debug - İzleme")]
        [SerializeField] private int _currentWaveNumber = 1;
        [SerializeField] private float _currentTotalBudget;
        [SerializeField] private bool _isSpawningInProgress = false;
        private bool _isResetting = false; 
        
        private HashSet<EnemyBehaviorController> _activeEnemiesRegistry = new HashSet<EnemyBehaviorController>();
        public List<EnemyDefinition> NextWaveEnemies { get; private set; } = new List<EnemyDefinition>();
        private WaveRule _currentRule;

        // --- EVENTLER ---
        public event System.Action OnWaveStarted;   
        public event System.Action OnWaveCompleted; 
        public event System.Action OnGameReset;     

        private void Start()
        {
            InitializeGame();
            StartCoroutine(FailsafeRoutine());
        }

        public void InitializeGame()
        {
            // 1. WAVE KAYDINI YÜKLE
            if (_savedWaveNumber != null && _savedWaveNumber.RuntimeValue > 0)
            {
                _currentWaveNumber = _savedWaveNumber.RuntimeValue;
            }
            else
            {
                _currentWaveNumber = 1;
                // Eğer kayıt yoksa veya 0 ise, varsayılanı kaydet
                if (_savedWaveNumber != null) _savedWaveNumber.RuntimeValue = 1;
            }

            // 2. BÜTÇE KAYDINI YÜKLE
            if (_savedBudget != null && _savedBudget.RuntimeValue > 0)
            {
                _currentTotalBudget = _savedBudget.RuntimeValue;
            }
            else
            {
                if (_config != null) _currentTotalBudget = _config.StartingBudget;
                // Varsayılanı kaydet
                if (_savedBudget != null) _savedBudget.RuntimeValue = _currentTotalBudget;
            }

            _activeEnemiesRegistry.Clear();
            _isResetting = false;

            Debug.Log($"💾 Oyun Yüklendi. Wave: {_currentWaveNumber}, Bütçe: {_currentTotalBudget}");
        }

        // --- SPAWNER İLETİŞİMİ ---
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

            // Cezalandır (Bütçe düşer ve KAYDEDİLİR)
            OnWaveLost();
            
            // Sahneyi Tamir Et
            OnGameReset?.Invoke(); 

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

            // [KAYIT İŞLEMİ] Yeni verileri diske (ScriptableObject'e) yaz
            if (_savedWaveNumber != null) _savedWaveNumber.RuntimeValue = _currentWaveNumber;
            if (_savedBudget != null) _savedBudget.RuntimeValue = _currentTotalBudget;

            OnGameReset?.Invoke(); 
            OnWaveCompleted?.Invoke();
        }

        public void OnWaveLost()
        {
            float penalty = _currentTotalBudget * _config.LossPenaltyPercentage;
            _currentTotalBudget -= penalty;
            if (_currentTotalBudget < _config.StartingBudget) _currentTotalBudget = _config.StartingBudget;

            // [KAYIT İŞLEMİ] Bütçe düştü, bunu da kaydet ki hile olmasın (çık-gir yapınca zorluk artmasın)
            if (_savedBudget != null) _savedBudget.RuntimeValue = _currentTotalBudget;
            
            // Not: Wave numarasını kaydetmiyoruz çünkü değişmedi.
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