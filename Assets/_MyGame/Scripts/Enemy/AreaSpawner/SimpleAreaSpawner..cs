using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D; 
using ArcadeBridge.ArcadeIdleEngine.Storage; 
using ArcadeBridge.ArcadeIdleEngine.Controller; 

namespace IndianOceanAssets.Engine2_5D.Spawners
{
    public class SimpleAreaSpawner : MonoBehaviour
    {
        [System.Serializable]
        public class SpawnWaveSettings
        {
            public string Name; 
            public EnemyDefinition EnemyType;
            public int SpawnCount = 5;
            public float SpawnInterval = 1f;
            public float StartDelay = 2f;
            
            [Header("🔴 Durum")]
            public int ActiveEnemies = 0;
            public int KillCount = 0;
            public string CurrentStatus = "Waiting";
        }

        [Header("🎒 Sistem")]
        [SerializeField] private Inventory _playerInventory;
        [SerializeField] private bool _autoFindPlayer = true;

        [Header("📊 İstatistik")]
        [SerializeField] private int _totalGlobalKills = 0;

        [Header("📋 Spawn Listesi")]
        [SerializeField] private List<SpawnWaveSettings> _spawnList;
        [SerializeField] private Vector3 _spawnAreaSize = new Vector3(10, 0, 10);

        private Dictionary<string, Queue<EnemyBehaviorController>> _poolDictionary = new Dictionary<string, Queue<EnemyBehaviorController>>();
        private bool _isApplicationQuitting = false;

        private void Start()
        {
            if (_autoFindPlayer) FindPlayerInventory();

            if (_spawnList != null)
            {
                foreach (var settings in _spawnList)
                {
                    if (settings.EnemyType != null)
                        StartCoroutine(ProcessWaveLoop(settings));
                }
            }
        }

        private void FindPlayerInventory()
        {
            var realPlayerController = FindObjectOfType<PlayerCannonController>();
            if (realPlayerController != null)
            {
                _playerInventory = realPlayerController.GetComponent<Inventory>();
                if (_playerInventory == null) _playerInventory = realPlayerController.GetComponentInChildren<Inventory>();
                if (_playerInventory == null) _playerInventory = realPlayerController.GetComponentInParent<Inventory>();
                
                if (_playerInventory != null) return;
            }

            if (_playerInventory == null)
            {
                GameObject tagObj = GameObject.FindGameObjectWithTag("Player");
                if (tagObj != null) _playerInventory = tagObj.GetComponentInChildren<Inventory>();
            }

            if (_playerInventory == null) StartCoroutine(RetryFindPlayer());
        }

        private IEnumerator RetryFindPlayer()
        {
            yield return new WaitForSeconds(1f);
            FindPlayerInventory();
        }

        private void OnApplicationQuit() => _isApplicationQuitting = true;

        private IEnumerator ProcessWaveLoop(SpawnWaveSettings wave)
        {
            while (true)
            {
                if (_isApplicationQuitting) yield break;

                wave.CurrentStatus = $"Waiting ({wave.StartDelay}s)...";
                yield return new WaitForSeconds(wave.StartDelay);

                wave.CurrentStatus = "Spawning...";
                int spawnedCount = 0;
                var waitInterval = new WaitForSeconds(wave.SpawnInterval);

                while (spawnedCount < wave.SpawnCount)
                {
                    if (_isApplicationQuitting) yield break;
                    SpawnEnemyForWave(wave);
                    spawnedCount++;
                    yield return waitInterval;
                }

                wave.CurrentStatus = "Battle in Progress...";
                while (wave.ActiveEnemies > 0)
                {
                    if (_isApplicationQuitting) yield break;
                    yield return null; 
                }

                wave.CurrentStatus = "Wave Cleared! Restarting...";
            }
        }

        private void SpawnEnemyForWave(SpawnWaveSettings wave)
        {
            if (_isApplicationQuitting) return;

            EnemyBehaviorController enemy = GetFromPool(wave.EnemyType);
            if (enemy == null) return;

            Vector3 randomPos = GetRandomPosition();
            enemy.transform.position = randomPos;
            enemy.transform.rotation = Quaternion.identity;
            
            enemy.InitializeEnemy(wave.EnemyType);
            wave.ActiveEnemies++;

            // Düşman ölünce çalışacak Callback
            enemy.OnReturnToPool = (deadEnemy) => 
            {
                if (_isApplicationQuitting) return;

                if (_playerInventory == null && _autoFindPlayer) FindPlayerInventory();

                // [KRİTİK DÜZELTME] Ganimet verilip verilmeyeceğini kontrol et
                CheckAndDropLoot(deadEnemy, wave.EnemyType);

                ReturnToPool(deadEnemy);
                
                if (wave != null) { wave.ActiveEnemies--; wave.KillCount++; }
                _totalGlobalKills++; 
            };
        }

        // [YENİ] Kontrol Mekanizması
        private void CheckAndDropLoot(EnemyBehaviorController enemy, EnemyDefinition data)
        {
            if (_isApplicationQuitting) return;
            
            // Eğer düşman öldüğünde "Benim ganimetim alındı" (LootDropped == true) diyorsa,
            // Demek ki Kule onu öldürmüş ve ganimetini almıştır. Biz oyuncuya vermiyoruz.
            if (enemy.LootDropped) return; 

            // Eğer alınmadıysa, demek ki Oyuncu öldürdü (veya başka bir sebeple öldü).
            // O zaman Oyuncunun çantasına yolla.
            TryDropLootToPlayer(enemy, data);
        }

        private void TryDropLootToPlayer(EnemyBehaviorController enemy, EnemyDefinition data)
        {
            if (_playerInventory == null || data.DropItem == null) return;
            if (!_playerInventory.CanAdd(data.DropItem)) return;

            var item = data.DropItem.Pool.Get();
            if (item != null)
            {
                item.transform.position = enemy.transform.position;
                item.gameObject.SetActive(true);
                item.transform.SetParent(null); 
                _playerInventory.Add(item);
            }
        }

        private Vector3 GetRandomPosition()
        {
            float x = Random.Range(-_spawnAreaSize.x / 2, _spawnAreaSize.x / 2);
            float z = Random.Range(-_spawnAreaSize.z / 2, _spawnAreaSize.z / 2);
            return transform.position + new Vector3(x, 0, z);
        }

        private EnemyBehaviorController GetFromPool(EnemyDefinition data)
        {
            if (data == null || data.EnemyPrefab == null) return null;
            string key = data.name;

            if (!_poolDictionary.ContainsKey(key)) _poolDictionary[key] = new Queue<EnemyBehaviorController>();

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
                if (!_poolDictionary.ContainsKey(key)) _poolDictionary[key] = new Queue<EnemyBehaviorController>();
                _poolDictionary[key].Enqueue(enemy);
            }
            else Destroy(enemy.gameObject);
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