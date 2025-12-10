using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D; // EnemyDefinition ve Controller için

namespace IndianOceanAssets.Engine2_5D.Spawners
{
    /// <summary>
    /// [TR] Her düşman türü için özel sayı ve hız ayarı yapılabilen gelişmiş alan spawner'ı.
    /// [EN] Advanced area spawner allowing individual count and speed settings for each enemy type.
    /// </summary>
    public class SimpleAreaSpawner : MonoBehaviour
    {
        [System.Serializable]
        public struct SpawnWaveSettings
        {
            [Header("Kim Doğacak?")]
            public string Name; // Editörde karışıklığı önlemek için (Örn: "Hızlı Goblinler")
            public EnemyDefinition EnemyType;

            [Header("Nasıl Doğacak?")]
            [Tooltip("Toplam kaç adet? (-1 yaparsan sonsuz doğar)")]
            public int SpawnCount;

            [Tooltip("Kaç saniyede bir doğsun?")]
            public float SpawnInterval;

            [Tooltip("Oyun başladıktan kaç saniye sonra doğmaya başlasın?")]
            public float StartDelay;
        }

        [Header("📋 Spawn Ayarları")]
        [Tooltip("Buraya istediğin kadar farklı düşman kuralı ekleyebilirsin.")]
        [SerializeField] private List<SpawnWaveSettings> _spawnList;

        [Header("📏 Alan Ayarları")]
        [SerializeField] private Vector3 _spawnAreaSize = new Vector3(10, 0, 10);

        // --- OPTİMİZE POOL (HAVUZ) SİSTEMİ ---
        private Dictionary<string, Queue<EnemyBehaviorController>> _poolDictionary = new Dictionary<string, Queue<EnemyBehaviorController>>();

        private void Start()
        {
            // Listendeki her bir kural için ayrı bir üretim döngüsü başlat
            if (_spawnList != null)
            {
                foreach (var settings in _spawnList)
                {
                    if (settings.EnemyType != null)
                    {
                        StartCoroutine(ProcessSpawnSettings(settings));
                    }
                }
            }
        }

        /// <summary>
        /// Her düşman ayarı için bağımsız çalışan döngü.
        /// </summary>
        private IEnumerator ProcessSpawnSettings(SpawnWaveSettings settings)
        {
            // 1. Başlangıç gecikmesi (Örn: Devler 10sn sonra gelsin)
            if (settings.StartDelay > 0) 
                yield return new WaitForSeconds(settings.StartDelay);

            int spawnedCount = 0;
            var waitInterval = new WaitForSeconds(settings.SpawnInterval);

            // 2. Üretim Döngüsü (-1 ise sonsuz, değilse sayıya kadar)
            while (settings.SpawnCount == -1 || spawnedCount < settings.SpawnCount)
            {
                SpawnSingleEnemy(settings.EnemyType);
                spawnedCount++;

                yield return waitInterval;
            }
        }

        private void SpawnSingleEnemy(EnemyDefinition data)
        {
            // Havuzdan veya yeni üretimle objeyi al
            EnemyBehaviorController enemy = GetFromPool(data);
            if (enemy == null) return;

            // Rastgele konum belirle
            Vector3 randomPos = GetRandomPosition();
            enemy.transform.position = randomPos;
            enemy.transform.rotation = Quaternion.identity; 

            // Düşmanı başlat (Canı, Hızı vb. yüklenir)
            enemy.InitializeEnemy(data);
        }

        private Vector3 GetRandomPosition()
        {
            float x = Random.Range(-_spawnAreaSize.x / 2, _spawnAreaSize.x / 2);
            float z = Random.Range(-_spawnAreaSize.z / 2, _spawnAreaSize.z / 2);
            return transform.position + new Vector3(x, 0, z);
        }

        // --- HAVUZ YÖNETİMİ ---
        private EnemyBehaviorController GetFromPool(EnemyDefinition data)
        {
            if (data == null || data.EnemyPrefab == null) return null;

            string key = data.name;

            if (!_poolDictionary.ContainsKey(key))
                _poolDictionary[key] = new Queue<EnemyBehaviorController>();

            // Havuzda varsa çek
            if (_poolDictionary[key].Count > 0)
            {
                EnemyBehaviorController pooled = _poolDictionary[key].Dequeue();
                if (pooled != null)
                {
                    pooled.OnReturnToPool = ReturnToPool;
                    return pooled;
                }
            }

            // Yoksa yeni yarat
            GameObject newObj = Instantiate(data.EnemyPrefab, transform);
            var controller = newObj.GetComponent<EnemyBehaviorController>();
            
            if (controller != null) controller.OnReturnToPool = ReturnToPool;
            
            newObj.SetActive(false);
            return controller;
        }

        private void ReturnToPool(EnemyBehaviorController enemy)
        {
            if (enemy == null) return;
            enemy.gameObject.SetActive(false);

            var stats = enemy.GetComponent<EnemyStats>();
            if (stats != null && stats.Definition != null)
            {
                string key = stats.Definition.name;
                if (!_poolDictionary.ContainsKey(key))
                    _poolDictionary[key] = new Queue<EnemyBehaviorController>();

                _poolDictionary[key].Enqueue(enemy);
            }
            else
            {
                Destroy(enemy.gameObject);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawCube(transform.position, new Vector3(_spawnAreaSize.x, 0.1f, _spawnAreaSize.z));
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, new Vector3(_spawnAreaSize.x, 0.1f, _spawnAreaSize.z));
        }
    }
}