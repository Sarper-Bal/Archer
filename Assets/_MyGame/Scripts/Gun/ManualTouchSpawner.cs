using UnityEngine;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D; // EnemyBehaviorController ve Stats için

namespace ArcadeBridge.ArcadeIdleEngine.Spawners
{
    public class ManualTouchSpawner : MonoBehaviour
    {
        [Header("⚙️ Ayarlar")]
        [Tooltip("Spawn edilecek birimin verisi")]
        [SerializeField] private EnemyDefinition _unitData;
        
        [Tooltip("Birimlerin doğacağı nokta")]
        [SerializeField] private Transform _spawnPoint;
        
        [Tooltip("Saniyede kaç birim çıkacak? (0.1 = Saniyede 10 tane)")]
        [SerializeField] private float _spawnInterval = 0.1f;

        [Header("🔧 Yön Ayarı (Mob Control Tarzı)")]
        [Tooltip("Birimler doğduğunda bu yöne gidecek")]
        [SerializeField] private Vector3 _moveDirection = new Vector3(0, 0, 1); 

        // --- Pooling (Performans) ---
        private Queue<EnemyBehaviorController> _pool = new Queue<EnemyBehaviorController>();
        private float _nextSpawnTime;

        private void Update()
        {
            // Mobilde ve PC'de çalışır. Basılı tutulduğu sürece true döner.
            if (Input.GetMouseButton(0))
            {
                TrySpawn();
            }
        }

        private void TrySpawn()
        {
            if (Time.time < _nextSpawnTime) return;

            SpawnUnit();
            _nextSpawnTime = Time.time + _spawnInterval;
        }

        private void SpawnUnit()
        {
            if (_unitData == null || _unitData.EnemyPrefab == null)
            {
                Debug.LogWarning("⚠️ Spawner: Unit Data veya Prefab eksik!");
                return;
            }

            EnemyBehaviorController unit = GetFromPool();
            
            // Pozisyon ve Rotasyon ayarla
            unit.transform.position = _spawnPoint != null ? _spawnPoint.position : transform.position;
            unit.transform.rotation = Quaternion.LookRotation(_moveDirection);

            // Datayı yükle (Stats)
            var stats = unit.GetComponent<EnemyStats>();
            if (stats != null)
            {
                stats.InitializeRuntime(_unitData);
                
                // [ÖNEMLİ] Birimin yönünü override et (Data'dan değil, Spawner'dan al)
                // Bu sayede tek bir EnemyType hem düşman hem dost olabilir.
                if (stats.Definition != null)
                {
                    // Not: ScriptableObject'i runtime'da değiştirmiyoruz, 
                    // sadece Mover scriptinin okuyacağı veriyi manipüle edebiliriz
                    // veya DirectionalMover'a direkt set edebiliriz.
                    // Şimdilik DirectionalMover data'dan okuduğu için data'daki yönün doğru olduğundan emin ol.
                }
            }

            // Birimi aktif et ve Davranışını 'Directional' yap
            unit.gameObject.SetActive(true);
            
            // Eğer EnemyDefinition'da default behavior 'Directional' değilse bile zorla:
            unit.SetBehavior(EnemyBehaviorType.Directional);
        }

        private EnemyBehaviorController GetFromPool()
        {
            if (_pool.Count > 0)
            {
                var pooledUnit = _pool.Dequeue();
                // Eğer havuzdaki obje silinmişse (destroy) yenisini yarat
                if (pooledUnit != null)
                {
                    pooledUnit.OnReturnToPool = ReturnToPool;
                    return pooledUnit;
                }
            }

            // Havuz boşsa yeni yarat
            GameObject newObj = Instantiate(_unitData.EnemyPrefab, transform);
            var controller = newObj.GetComponent<EnemyBehaviorController>();
            
            // Directional Mover yoksa ekle (Güvenlik)
            if (newObj.GetComponent<DirectionalEnemyMover>() == null)
            {
                newObj.AddComponent<DirectionalEnemyMover>();
            }

            controller.OnReturnToPool = ReturnToPool;
            newObj.SetActive(false);
            return controller;
        }

        private void ReturnToPool(EnemyBehaviorController unit)
        {
            unit.gameObject.SetActive(false);
            _pool.Enqueue(unit);
        }
    }
}