using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D; // EnemyBehaviorController için
using IndianOceanAssets.Engine2_5D.Spawners; // Veri yapıları için
using ArcadeBridge.ArcadeIdleEngine.Enemy; // Route için

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
        [SerializeField] private bool _waitingForCleave = false; // Düşmanların ölmesini mi bekliyor?
        
        // Aktif düşmanları takip listesi
        private List<EnemyBehaviorController> _activeEnemies = new List<EnemyBehaviorController>();

        // Dışarıdan erişim eventleri (UI için harika olur: "Wave 1 Başladı!" yazısı gibi)
        public System.Action<int, int> OnWaveChanged; // (Mevcut Dalga, Toplam Dalga)
        public System.Action OnAllWavesComplete;

        private void Start()
        {
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
                // Config bitti mi?
                if (_currentWaveIndex >= _waveConfig.Waves.Count)
                {
                    if (_waveConfig.LoopWaves)
                    {
                        _currentWaveIndex = 0; // Başa dön
                    }
                    else
                    {
                        Debug.Log("🎉 Tüm dalgalar tamamlandı!");
                        OnAllWavesComplete?.Invoke();
                        yield break; // Coroutine'i bitir
                    }
                }

                // --- DALGA BAŞLIYOR ---
                WaveDefinition currentWave = _waveConfig.Waves[_currentWaveIndex];
                OnWaveChanged?.Invoke(_currentWaveIndex + 1, _waveConfig.Waves.Count);
                Debug.Log($"🌊 Dalga Başladı: {currentWave.WaveName}");

                // 1. Grupları Üret
                _isSpawning = true;
                foreach (var group in currentWave.Groups)
                {
                    yield return StartCoroutine(SpawnGroupRoutine(group));
                }
                _isSpawning = false;

                // 2. Bekleme Mantığı (Hepsi ölsün mü?)
                if (currentWave.WaitForAllDead)
                {
                    _waitingForCleave = true;
                    // Listede canlı düşman kaldığı sürece bekle
                    while (HasActiveEnemies())
                    {
                        yield return new WaitForSeconds(0.5f); // Optimizasyon: Her frame değil, yarım saniyede bir kontrol et
                    }
                    _waitingForCleave = false;
                }

                // 3. Mola (Sonraki dalgaya geçiş süresi)
                if (currentWave.TimeToNextWave > 0)
                {
                    Debug.Log($"⏳ Mola: {currentWave.TimeToNextWave} saniye...");
                    yield return new WaitForSeconds(currentWave.TimeToNextWave);
                }

                _currentWaveIndex++;
            }
        }

        private IEnumerator SpawnGroupRoutine(WaveGroup group)
        {
            if (group.EnemyPool == null)
            {
                Debug.LogError($"Hata: {name} üzerindeki bir grup için Pool atanmamış!");
                yield break;
            }

            for (int i = 0; i < group.Count; i++)
            {
                SpawnEnemy(group.EnemyPool);
                
                if (group.DelayBetweenSpawns > 0)
                    yield return new WaitForSeconds(group.DelayBetweenSpawns);
            }
        }

        private void SpawnEnemy(EnemyPool pool)
        {
            EnemyBehaviorController enemy = pool.Get();

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

        private bool HasActiveEnemies()
        {
            // Listeyi temizle (Ölüleri at)
            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                if (_activeEnemies[i] == null || !_activeEnemies[i].gameObject.activeSelf)
                {
                    _activeEnemies.RemoveAt(i);
                }
            }

            return _activeEnemies.Count > 0;
        }

        // Görselleştirme
        private void OnDrawGizmos()
        {
            Gizmos.color = _waitingForCleave ? Color.yellow : (_isSpawning ? Color.green : Color.red);
            Gizmos.DrawWireCube(transform.position, _spawnAreaSize);
        }
    }
}