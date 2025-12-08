using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D.Managers; 
using ArcadeBridge.ArcadeIdleEngine.Items;    
using ArcadeBridge.ArcadeIdleEngine.Storage;  
using IndianOceanAssets.Engine2_5D; // EnemyBehaviorController için
using UnityEngine.AI; 

namespace ArcadeBridge.ArcadeIdleEngine.Interactables
{
    public class InventoryUnitSpawner : MonoBehaviour
    {
        [Header("🔗 Bağlantılar")]
        [Tooltip("Askerlerin stoklandığı envanter.")]
        [SerializeField] private Inventory _targetInventory;
        
        [Tooltip("Canlı askerlerin doğacağı nokta.")]
        [SerializeField] private Transform _spawnPoint;
        
        [Header("⚔️ Asker Ayarları")]
        [Tooltip("Üretilecek Dost Asker Prefabı")]
        [SerializeField] private GameObject _allyPrefab;
        
        [Tooltip("Hangi eşya 1 askere dönüşecek?")]
        [SerializeField] private ItemDefinition _unitItemDef;
        
        [Tooltip("Savaş anında kaç saniyede bir asker çıksın?")]
        [SerializeField] private float _spawnInterval = 1.0f;

        [Header("⚙️ Havuz Ayarları")]
        [Tooltip("Başlangıçta kaç asker hazır beklesin?")]
        [SerializeField] private int _initialPoolSize = 10;

        // --- İÇ HAVUZ SİSTEMİ (Internal Pool) ---
        private Queue<GameObject> _poolQueue = new Queue<GameObject>();
        private Transform _poolContainer; // Hiyerarşide düzenli dursunlar diye

        private SmartWaveManager _waveManager;
        private Coroutine _spawnRoutine;
        private bool _isSpawningActive = false;

        private void Awake()
        {
            _waveManager = FindObjectOfType<SmartWaveManager>();
            if (_targetInventory == null) _targetInventory = GetComponent<Inventory>();

            // Havuz için objenin altında bir klasör (Container) oluştur
            GameObject container = new GameObject("Unit_Pool_Container");
            container.transform.SetParent(transform);
            _poolContainer = container.transform;

            // Başlangıç havuzunu oluştur
            InitializeInternalPool();
        }

        private void Start()
        {
            if (_waveManager != null)
            {
                _waveManager.OnWaveStarted += StartSpawning;
                _waveManager.OnWaveCompleted += StopSpawning;
                _waveManager.OnGameReset += StopSpawning;
            }
        }

        private void OnDestroy()
        {
            if (_waveManager != null)
            {
                _waveManager.OnWaveStarted -= StartSpawning;
                _waveManager.OnWaveCompleted -= StopSpawning;
                _waveManager.OnGameReset -= StopSpawning;
            }
        }

        // --- HAVUZ YÖNETİMİ (Kritik Kısım) ---

        private void InitializeInternalPool()
        {
            if (_allyPrefab == null)
            {
                Debug.LogError("❌ InventoryUnitSpawner: Asker Prefabı (Ally Prefab) eksik!");
                return;
            }

            for (int i = 0; i < _initialPoolSize; i++)
            {
                CreateNewUnitForPool();
            }
        }

        private GameObject CreateNewUnitForPool()
        {
            // Askeri yarat ve container'ın içine koy
            GameObject unit = Instantiate(_allyPrefab, _poolContainer);
            
            // Dönüş mekanizmasını bağla (EnemyBehaviorController kullanıyorlarsa)
            var controller = unit.GetComponent<EnemyBehaviorController>();
            if (controller != null)
            {
                controller.OnReturnToPool = ReturnUnitToPool;
            }

            // Pasif yap ve kuyruğa ekle
            unit.SetActive(false);
            _poolQueue.Enqueue(unit);
            return unit;
        }

        private void ReturnUnitToPool(EnemyBehaviorController unitController)
        {
            // Asker öldüğünde veya işi bittiğinde buraya gelecek
            GameObject unitObj = unitController.gameObject;
            unitObj.SetActive(false);
            unitObj.transform.SetParent(_poolContainer); // Yuvaya dön
            unitObj.transform.localPosition = Vector3.zero; // Temizlik
            
            _poolQueue.Enqueue(unitObj);
        }

        // --- SPAWN İŞLEMLERİ ---

        private void StartSpawning()
        {
            _isSpawningActive = true;
            if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
            _spawnRoutine = StartCoroutine(SpawnProcess());
        }

        private void StopSpawning()
        {
            _isSpawningActive = false;
            if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
        }

        private IEnumerator SpawnProcess()
        {
            WaitForSeconds wait = new WaitForSeconds(_spawnInterval);

            while (_isSpawningActive)
            {
                // Envanter kontrolü
                if (_targetInventory != null && _targetInventory.TryRemove(_unitItemDef, out Item removedItem))
                {
                    // "Asker Paketi" eşyasını yok et (Tüket)
                    removedItem.ReleaseToPool();

                    // "Canlı Asker" çağır
                    SpawnLiveUnit();
                }

                yield return wait;
            }
        }

        private void SpawnLiveUnit()
        {
            if (_spawnPoint == null) return;

            GameObject unit;

            // 1. Havuzdan Çek (Yoksa Yeni Yarat)
            if (_poolQueue.Count > 0)
            {
                unit = _poolQueue.Dequeue();
                // Eğer havuzdaki obje silinmişse (hata durumunda) yenisini yap
                if (unit == null) unit = CreateNewUnitForPool(); 
            }
            else
            {
                // Havuz boşaldı! Dinamik olarak yeni bir tane üret (Limit yok)
                // Ama kuyruğa eklemiyoruz, direkt kullanıyoruz.
                unit = Instantiate(_allyPrefab, _poolContainer);
                var controller = unit.GetComponent<EnemyBehaviorController>();
                if (controller != null) controller.OnReturnToPool = ReturnUnitToPool;
            }

            // 2. [ÖNEMLİ] Önce Pozisyonu Ayarla (Obje hala inaktif olabilir)
            unit.transform.position = _spawnPoint.position;
            unit.transform.rotation = _spawnPoint.rotation;

            // 3. NavMeshAgent Reset (Warp)
            var agent = unit.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.enabled = false; // Garanti olsun diye kapat
                unit.transform.position = _spawnPoint.position; // Transformu zorla
                agent.enabled = true;  // Aç
                if (agent.isOnNavMesh) agent.Warp(_spawnPoint.position); // Işınla
            }

            // 4. [FİNAL] Artık her şey hazır, askeri uyandır!
            unit.SetActive(true);

            // Eğer özel bir başlatma/reset kodu varsa (Can doldurma vb.)
            var stats = unit.GetComponent<EnemyStats>();
            if (stats != null) 
            {
                 // stats.InitializeRuntime(...) gerekebilir eğer canı dolmuyorsa
                 // Ama genelde OnEnable bunu halleder.
                 var health = unit.GetComponent<Health>();
                 if(health) health.ResetHealth(); // Canını fulle
            }
        }
    }
}