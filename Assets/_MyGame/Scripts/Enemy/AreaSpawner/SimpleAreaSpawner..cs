using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D; 
using ArcadeBridge.ArcadeIdleEngine.Storage; // Inventory ve Item için
using ArcadeBridge.ArcadeIdleEngine.Controller; // PlayerCannonController için

namespace IndianOceanAssets.Engine2_5D.Spawners
{
    public class SimpleAreaSpawner : MonoBehaviour
    {
        // --- AYARLAR ---
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
        // [DÜZELTME 1] Türü 'InventoryVisible' değil 'Inventory' yaptık.
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
            if (_autoFindPlayer)
            {
                FindPlayerInventory();
            }

            if (_spawnList != null)
            {
                foreach (var settings in _spawnList)
                {
                    if (settings.EnemyType != null)
                        StartCoroutine(ProcessWaveLoop(settings));
                }
            }
        }

        // [DÜZELTME 2] En doğru envanteri bulma algoritması
        private void FindPlayerInventory()
        {
            // Adım 1: Gerçek kontrolcüyü (Hareket eden objeyi) bul
            var realPlayerController = FindObjectOfType<PlayerCannonController>();
            
            if (realPlayerController != null)
            {
                // Önce kontrolcünün olduğu objeye bak (En garantisi budur)
                _playerInventory = realPlayerController.GetComponent<Inventory>();

                // Eğer orada yoksa, hemen altındaki çocuklara bak (Visuals içinde olabilir)
                if (_playerInventory == null)
                    _playerInventory = realPlayerController.GetComponentInChildren<Inventory>();

                // Eğer hala yoksa, Ebeveynine bak (Bazen Root'ta olur)
                if (_playerInventory == null)
                    _playerInventory = realPlayerController.GetComponentInParent<Inventory>();

                if (_playerInventory != null)
                {
                    Debug.Log($"✅ Spawner: Gerçek Envanter bulundu: {_playerInventory.name}");
                    return;
                }
            }

            // Eğer kontrolcü yoksa (Test sahnesi vb.) Tag ile dene
            if (_playerInventory == null)
            {
                GameObject tagObj = GameObject.FindGameObjectWithTag("Player");
                if (tagObj != null) _playerInventory = tagObj.GetComponentInChildren<Inventory>();
            }

            // Bulamazsa tekrar dene
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

            enemy.OnReturnToPool = (deadEnemy) => 
            {
                if (_isApplicationQuitting) return;

                // Referans kontrolü (Kaybolduysa tekrar bul)
                if (_playerInventory == null && _autoFindPlayer) FindPlayerInventory();

                TryDropLoot(deadEnemy, wave.EnemyType);
                ReturnToPool(deadEnemy);
                
                if (wave != null) { wave.ActiveEnemies--; wave.KillCount++; }
                _totalGlobalKills++; 
            };
        }

        private void TryDropLoot(EnemyBehaviorController enemy, EnemyDefinition data)
        {
            if (_isApplicationQuitting || _playerInventory == null || data.DropItem == null) return;
            
            // [DÜZELTME 3] Inventory sınıfının kendi CanAdd kontrolünü kullan
            if (!_playerInventory.CanAdd(data.DropItem)) return;

            var item = data.DropItem.Pool.Get();
            if (item != null)
            {
                item.transform.position = enemy.transform.position;
                item.gameObject.SetActive(true);
                item.transform.SetParent(null); 
                
                // [DÜZELTME 4] Inventory.Add metodu, Visible/Invisible ayrımını kendi yapar
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