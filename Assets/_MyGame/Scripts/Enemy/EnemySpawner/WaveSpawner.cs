using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D; 
using IndianOceanAssets.Engine2_5D.Spawners; 
using ArcadeBridge.ArcadeIdleEngine.Enemy; 

namespace ArcadeBridge.ArcadeIdleEngine.Spawners
{
    public class WaveSpawner : MonoBehaviour
    {
        [Header("Data (Beyin)")]
        [SerializeField] private WaveConfig _waveConfig;

        [Header("Alan Ayarları")]
        [SerializeField] private Vector3 _spawnAreaSize = new Vector3(5, 0, 5);
        [SerializeField] private WaypointRoute _forcePatrolRoute;

        [Header("Durum (Debug)")]
        [SerializeField] private int _currentWaveIndex = 0;
        [SerializeField] private bool _isSpawning = false;
        [SerializeField] private bool _waitingForCleave = false;
        
        // Aktif düşmanları takip listesi
        private List<EnemyBehaviorController> _activeEnemies = new List<EnemyBehaviorController>();

        // [OPTİMİZASYON 1] Çöp oluşumunu (GC) engellemek için cache'lenmiş bekleme objeleri
        private WaitForSeconds _checkInterval; 
        private WaitForSeconds _groupDelay;

        // Eventler
        public System.Action<int, int> OnWaveChanged; 
        public System.Action OnAllWavesComplete;

        private void Start()
        {
            // [OPTİMİZASYON 1] Objeleri sadece oyun başında 1 kere yarat
            _checkInterval = new WaitForSeconds(0.5f); 

            if (_waveConfig != null)
            {
                StartCoroutine(ProcessWaves());
            }
        }

        private IEnumerator ProcessWaves()
        {
            _currentWaveIndex = 0;

            while (true)
            {
                // Config bitti mi kontrolü
                if (_currentWaveIndex >= _waveConfig.Waves.Count)
                {
                    if (_waveConfig.LoopWaves)
                    {
                        _currentWaveIndex = 0; 
                    }
                    else
                    {
                        Debug.Log("🎉 Tüm dalgalar tamamlandı!");
                        OnAllWavesComplete?.Invoke();
                        yield break; 
                    }
                }

                WaveDefinition currentWave = _waveConfig.Waves[_currentWaveIndex];
                OnWaveChanged?.Invoke(_currentWaveIndex + 1, _waveConfig.Waves.Count);
                
                // [OPTİMİZASYON 2] Yeni dalga başlamadan önce listeyi temizle (Toplu Temizlik)
                CleanupDeadEnemiesImmediately(); 

                Debug.Log($"🌊 Dalga Başladı: {currentWave.WaveName}");

                // 1. Düşmanları Üret
                _isSpawning = true;
                foreach (var group in currentWave.Groups)
                {
                    yield return StartCoroutine(SpawnGroupRoutine(group));
                }
                _isSpawning = false;

                // 2. Bekleme Mantığı (Ultra Optimize)
                if (currentWave.WaitForAllDead)
                {
                    _waitingForCleave = true;
                    
                    // Döngü içinde listeyi modifiye etmiyoruz (RemoveAt yok).
                    // Sadece "Hala yaşayan var mı?" diye soruyoruz. Bu çok hızlıdır.
                    while (IsAnyEnemyAlive())
                    {
                        // Cachelenmiş wait kullanımı (Sıfır GC)
                        yield return _checkInterval; 
                    }
                    _waitingForCleave = false;
                }

                // 3. Mola
                if (currentWave.TimeToNextWave > 0)
                {
                    yield return new WaitForSeconds(currentWave.TimeToNextWave);
                }

                _currentWaveIndex++;
            }
        }

        private IEnumerator SpawnGroupRoutine(WaveGroup group)
        {
            if (group.EnemyPool == null) yield break;

            // Grup içi bekleme süresini cache'leyelim (Eğer sabitse)
            WaitForSeconds groupSpawnDelay = new WaitForSeconds(group.DelayBetweenSpawns);

            for (int i = 0; i < group.Count; i++)
            {
                SpawnEnemy(group.EnemyPool);
                
                if (group.DelayBetweenSpawns > 0)
                    yield return groupSpawnDelay;
            }
        }

       private void SpawnEnemy(EnemyPool pool)
        {
            // Havuzdan al
            EnemyBehaviorController enemy = pool.Get();

            // [ÇÖZÜM] Düşmana sahibini tanıt (Çok önemli satır!)
            enemy.InitializePool(pool); 

            // Pozisyon
            Vector3 randomOffset = new Vector3(
                Random.Range(-_spawnAreaSize.x / 2, _spawnAreaSize.x / 2),
                0,
                Random.Range(-_spawnAreaSize.z / 2, _spawnAreaSize.z / 2)
            );
            enemy.transform.position = transform.position + randomOffset;
            enemy.transform.rotation = transform.rotation;

            // Rota
            if (_forcePatrolRoute != null)
            {
                enemy.SetPatrolRoute(_forcePatrolRoute);
            }

            _activeEnemies.Add(enemy);
        }

        // [OPTİMİZASYON 3] Bu fonksiyon sadece okuma yapar, yazma/silme yapmaz. O(N) ama çok hafif.
        private bool IsAnyEnemyAlive()
        {
            for (int i = 0; i < _activeEnemies.Count; i++)
            {
                // Eğer referans null değilse VE obje aktifse, hala yaşayan var demektir.
                if (_activeEnemies[i] != null && _activeEnemies[i].gameObject.activeSelf)
                {
                    return true; // Bir tane bulduk, döngüyü kır ve çık.
                }
            }
            return false; // Hiçbiri aktif değil.
        }

        // Listeyi sadece dalga geçişlerinde toplu temizleriz.
        private void CleanupDeadEnemiesImmediately()
        {
            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                if (_activeEnemies[i] == null || !_activeEnemies[i].gameObject.activeSelf)
                {
                    _activeEnemies.RemoveAt(i);
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _waitingForCleave ? Color.yellow : (_isSpawning ? Color.green : Color.red);
            Gizmos.DrawWireCube(transform.position, _spawnAreaSize);
        }
    }
}