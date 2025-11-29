using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Enemy;
using IndianOceanAssets.Engine2_5D;
using System.Collections.Generic;

namespace ArcadeBridge.ArcadeIdleEngine.Spawners
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Havuz Ayarları")]
        [Tooltip("Hangi düşman tipini doğuracak? (Proje klasöründeki Pool dosyası)")]
        [SerializeField] private EnemyPool _enemyPool;

        [Header("Spawn Mantığı")]
        [SerializeField] private float _spawnInterval = 3.0f; // Kaç saniyede bir?
        [SerializeField] private int _maxActiveEnemies = 5;   // Aynı anda en fazla kaç tane?
        [SerializeField] private bool _spawnOnStart = true;

        [Header("Alan Ayarları")]
        [SerializeField] private Vector3 _spawnAreaSize = new Vector3(5, 0, 5); // Alan genişliği

        [Header("Opsiyonel: Devriye Görevi")]
        [Tooltip("Eğer atanırsa, doğan düşmanlar bu yolu takip eder (Patrol Moduna geçer).")]
        [SerializeField] private WaypointRoute _forcePatrolRoute;

        // Canlı düşmanları takip listesi
        private List<EnemyBehaviorController> _activeEnemies = new List<EnemyBehaviorController>();
        private float _nextSpawnTime;

        private void Start()
        {
            if (_spawnOnStart)
                _nextSpawnTime = Time.time + 0.5f; // Başlangıçta minik bir gecikme iyidir
                
            // Listeyi temizle (Reload durumları için)
            _activeEnemies.Clear();
        }

        private void Update()
        {
            // 1. Ölüleri listeden temizle (Optimizasyon: Tersten döngü)
            CleanupDeadEnemies();

            // 2. Zamanı ve Limiti Kontrol Et
            if (Time.time >= _nextSpawnTime)
            {
                if (_activeEnemies.Count < _maxActiveEnemies)
                {
                    SpawnEnemy();
                }
                
                _nextSpawnTime = Time.time + _spawnInterval;
            }
        }

        private void SpawnEnemy()
        {
            if (_enemyPool == null)
            {
                Debug.LogWarning($"{name}: EnemyPool atanmamış!");
                return;
            }

            // --- HAVUZDAN ÇEKME (ALLOCATION YOK) ---
            EnemyBehaviorController enemy = _enemyPool.Get();

            // 1. Pozisyon Ayarla
            Vector3 randomOffset = new Vector3(
                Random.Range(-_spawnAreaSize.x / 2, _spawnAreaSize.x / 2),
                0,
                Random.Range(-_spawnAreaSize.z / 2, _spawnAreaSize.z / 2)
            );
            
            // Spawner'ın olduğu yere ofseti ekle
            enemy.transform.position = transform.position + randomOffset;
            enemy.transform.rotation = transform.rotation;

            // 2. Eğer özel bir yol (Route) atandıysa emri ver
            if (_forcePatrolRoute != null)
            {
                enemy.SetPatrolRoute(_forcePatrolRoute);
            }
            
            // 3. Listeye kaydet ve takip et
            _activeEnemies.Add(enemy);
        }

        private void CleanupDeadEnemies()
        {
            // Listenin içindeki düşmanların hala aktif olup olmadığını kontrol et
            // Eğer "SetActive(false)" olmuşlarsa listeden düş.
            // (Not: Listeden çıkarma işlemi CPU yiyebilir, bunu her karede yapmayabiliriz 
            // ama 5-10 düşman için sorun olmaz).
            
            for (int i = _activeEnemies.Count - 1; i >= 0; i--)
            {
                if (_activeEnemies[i] == null || !_activeEnemies[i].gameObject.activeSelf)
                {
                    _activeEnemies.RemoveAt(i);
                }
            }
        }

        // Editörde alanı çiz
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f); // Kırmızı şeffaf kutu
            Gizmos.DrawCube(transform.position, _spawnAreaSize);
            Gizmos.DrawWireCube(transform.position, _spawnAreaSize);
        }
    }
}