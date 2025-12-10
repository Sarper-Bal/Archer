using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D; 

namespace IndianOceanAssets.Engine2_5D.Spawners
{
    public class SimpleAreaSpawner : MonoBehaviour
    {
        // --- AYARLAR VE DATA ---
        [System.Serializable]
        public class SpawnWaveSettings
        {
            [Header("Kim Doğacak?")]
            public string Name; 
            public EnemyDefinition EnemyType;

            [Header("Nasıl Doğacak?")]
            [Tooltip("Toplam kaç adet doğacak?")]
            public int SpawnCount = 5;

            [Tooltip("Kaç saniyede bir doğsun?")]
            public float SpawnInterval = 1f;

            [Tooltip("Dalga başlamadan önce (veya tekrar etmeden önce) kaç saniye beklensin?")]
            public float StartDelay = 2f;

            [Header("🔴 Canlı Takip (Değiştirme)")]
            [Tooltip("Şu an sahnede canlı olan düşman sayısı.")]
            public int ActiveEnemies = 0;
            
            [Tooltip("Bu gruptan toplam kaç düşman öldürüldü?")]
            public int KillCount = 0;
            
            [Tooltip("Şu anki durumu gösterir.")]
            public string CurrentStatus = "Waiting";
        }

        [Header("📊 Genel İstatistikler")]
        [Tooltip("Tüm gruplardan toplam öldürülen düşman sayısı.")]
        [SerializeField] private int _totalGlobalKills = 0;

        [Header("📋 Spawn Ayarları")]
        [Tooltip("Buraya istediğin kadar farklı düşman kuralı ekleyebilirsin.")]
        [SerializeField] private List<SpawnWaveSettings> _spawnList;

        [Header("📏 Alan Ayarları")]
        [SerializeField] private Vector3 _spawnAreaSize = new Vector3(10, 0, 10);

        // --- POOL SİSTEMİ ---
        private Dictionary<string, Queue<EnemyBehaviorController>> _poolDictionary = new Dictionary<string, Queue<EnemyBehaviorController>>();

        private void Start()
        {
            if (_spawnList != null)
            {
                // Her ayar grubu için ayrı bir "Yönetici Coroutine" başlat
                foreach (var settings in _spawnList)
                {
                    if (settings.EnemyType != null)
                    {
                        StartCoroutine(ProcessWaveLoop(settings));
                    }
                }
            }
        }

        /// <summary>
        /// [YENİ] Sonsuz döngü mantığı: Bekle -> Doğur -> Ölmesini Bekle -> Başa Dön
        /// </summary>
        private IEnumerator ProcessWaveLoop(SpawnWaveSettings wave)
        {
            // Sonsuz döngü
            while (true)
            {
                // 1. Bekleme Aşaması
                wave.CurrentStatus = $"Waiting ({wave.StartDelay}s)...";
                yield return new WaitForSeconds(wave.StartDelay);

                // 2. Doğurma Aşaması
                wave.CurrentStatus = "Spawning...";
                int spawnedCount = 0;
                var waitInterval = new WaitForSeconds(wave.SpawnInterval);

                while (spawnedCount < wave.SpawnCount)
                {
                    SpawnEnemyForWave(wave);
                    spawnedCount++;
                    yield return waitInterval;
                }

                // 3. Savaş Aşaması (Hepsi ölene kadar bekle)
                wave.CurrentStatus = "Battle in Progress...";
                
                // ActiveEnemies 0 olana kadar her frame bekle
                while (wave.ActiveEnemies > 0)
                {
                    yield return null; 
                }

                // 4. Bitiş ve Tekrar
                wave.CurrentStatus = "Wave Cleared! Restarting...";
                // Döngü başa döner ve tekrar StartDelay kadar bekler
            }
        }

        private void SpawnEnemyForWave(SpawnWaveSettings wave)
        {
            EnemyBehaviorController enemy = GetFromPool(wave.EnemyType);
            if (enemy == null) return;

            // Rastgele konum
            Vector3 randomPos = GetRandomPosition();
            enemy.transform.position = randomPos;
            enemy.transform.rotation = Quaternion.identity;

            // Düşmanı başlat
            enemy.InitializeEnemy(wave.EnemyType);

            // [SAYAÇ GÜNCELLEME]
            wave.ActiveEnemies++;

            // [ÖZEL CALLBACK] Düşman öldüğünde bu fonksiyon çalışacak
            enemy.OnReturnToPool = (deadEnemy) => 
            {
                // 1. Standart havuz işlemi (Objeyi kapat ve sakla)
                ReturnToPool(deadEnemy);

                // 2. Sayaçları güncelle
                wave.ActiveEnemies--;
                wave.KillCount++;
                _totalGlobalKills++; // Genel sayacı artır
            };
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

            if (_poolDictionary[key].Count > 0)
            {
                EnemyBehaviorController pooled = _poolDictionary[key].Dequeue();
                if (pooled != null) return pooled;
            }

            GameObject newObj = Instantiate(data.EnemyPrefab, transform);
            var controller = newObj.GetComponent<EnemyBehaviorController>();
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