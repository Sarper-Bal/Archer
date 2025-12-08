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
        [SerializeField] private int _initialPoolSize = 10;

        // --- İÇ HAVUZ ve TAKİP LİSTESİ ---
        private Queue<GameObject> _poolQueue = new Queue<GameObject>();
        private HashSet<GameObject> _activeAlliesList = new HashSet<GameObject>(); // Sahnedeki askerleri takip eder
        private Transform _poolContainer; 

        private SmartWaveManager _waveManager;
        private Coroutine _spawnRoutine;
        private bool _isSpawningActive = false;

        private void Awake()
        {
            _waveManager = FindObjectOfType<SmartWaveManager>();
            if (_targetInventory == null) _targetInventory = GetComponent<Inventory>();

            // Havuz Container'ı
            GameObject container = new GameObject("Unit_Pool_Container");
            container.transform.SetParent(transform);
            _poolContainer = container.transform;
            
            // Konumunu sıfırla ki içinde oluşanlar saçma yerlere gitmesin
            _poolContainer.localPosition = Vector3.zero; 

            InitializeInternalPool();
        }

        private void Start()
        {
            if (_waveManager != null)
            {
                _waveManager.OnWaveStarted += StartSpawning;
                _waveManager.OnWaveCompleted += OnWaveEnded; // [YENİ] Temizlik için
                _waveManager.OnGameReset += OnWaveEnded;
            }
        }

        private void OnDestroy()
        {
            if (_waveManager != null)
            {
                _waveManager.OnWaveStarted -= StartSpawning;
                _waveManager.OnWaveCompleted -= OnWaveEnded;
                _waveManager.OnGameReset -= OnWaveEnded;
            }
        }

        // --- HAVUZ YÖNETİMİ ---

        private void InitializeInternalPool()
        {
            if (_allyPrefab == null) return;
            for (int i = 0; i < _initialPoolSize; i++) CreateNewUnitForPool();
        }

        private GameObject CreateNewUnitForPool()
        {
            GameObject unit = Instantiate(_allyPrefab, _poolContainer);
            
            var controller = unit.GetComponent<EnemyBehaviorController>();
            if (controller != null) controller.OnReturnToPool = ReturnUnitToPool;

            unit.SetActive(false);
            _poolQueue.Enqueue(unit);
            return unit;
        }

        private void ReturnUnitToPool(EnemyBehaviorController unitController)
        {
            if (unitController == null) return;
            
            GameObject unitObj = unitController.gameObject;
            
            // Listeden düş (Artık sahnede değil)
            if (_activeAlliesList.Contains(unitObj)) _activeAlliesList.Remove(unitObj);

            unitObj.SetActive(false);
            unitObj.transform.SetParent(_poolContainer);
            unitObj.transform.localPosition = Vector3.zero; 
            
            _poolQueue.Enqueue(unitObj);
        }

        // --- SPAWN VE TEMİZLİK ---

        private void StartSpawning()
        {
            _isSpawningActive = true;
            if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
            _spawnRoutine = StartCoroutine(SpawnProcess());
        }

        private void OnWaveEnded()
        {
            // 1. Spawner'ı durdur
            _isSpawningActive = false;
            if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);

            // 2. [YENİ] SAHNEDEKİ TÜM DOST ASKERLERİ TEMİZLE
            ClearAllActiveUnits();
        }

        private void ClearAllActiveUnits()
        {
            // Listeyi kopyala çünkü döngü içinde liste değişecek (ReturnUnitToPool çağrılınca)
            var unitsToClear = new List<GameObject>(_activeAlliesList);
            
            foreach (var unit in unitsToClear)
            {
                if (unit != null)
                {
                    // Askeri havuza geri yolla (Manuel tetikleme)
                    var controller = unit.GetComponent<EnemyBehaviorController>();
                    if (controller != null) ReturnUnitToPool(controller);
                    else
                    {
                        // Controller yoksa manuel kapat
                        unit.SetActive(false);
                        unit.transform.SetParent(_poolContainer);
                        _poolQueue.Enqueue(unit);
                    }
                }
            }
            _activeAlliesList.Clear();
            Debug.Log("🧹 Dost birlikler geri çekildi.");
        }

        private IEnumerator SpawnProcess()
        {
            WaitForSeconds wait = new WaitForSeconds(_spawnInterval);

            while (_isSpawningActive)
            {
                if (_targetInventory != null && _targetInventory.TryRemove(_unitItemDef, out Item removedItem))
                {
                    removedItem.ReleaseToPool();
                    SpawnLiveUnit();
                }
                yield return wait;
            }
        }

        private void SpawnLiveUnit()
        {
            if (_spawnPoint == null) return;

            GameObject unit;

            // 1. Havuzdan Çek (Objenin KAPALI gelmesi garanti)
            if (_poolQueue.Count > 0)
            {
                unit = _poolQueue.Dequeue();
                if (unit == null) unit = CreateNewUnitForPool();
            }
            else
            {
                unit = Instantiate(_allyPrefab, _poolContainer);
                unit.SetActive(false); // Yeni yaratılanı hemen kapat ki ayar yapabilelim
                var controller = unit.GetComponent<EnemyBehaviorController>();
                if (controller != null) controller.OnReturnToPool = ReturnUnitToPool;
            }

            // 2. Takip Listesine Ekle
            _activeAlliesList.Add(unit);

            // 3. [KRİTİK] Pozisyonlama (Obje hala KAPALI)
            // Önce Transform'u taşı
            unit.transform.position = _spawnPoint.position;
            unit.transform.rotation = _spawnPoint.rotation;

            // 4. NavMeshAgent Reset (Warp)
            // Agent kapalıyken Warp çalışmaz, ama obje kapalıyken Agent'ı açamayız.
            // Bu yüzden önce transformu ayarladık, şimdi objeyi açıp hemen Warp atacağız.
            
            unit.SetActive(true); // <--- Obje burada açılıyor

            var agent = unit.GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.enabled = true;
                // Warp, agent'ı navmesh üzerindeki en yakın geçerli noktaya ışınlar.
                // SpawnPoint havada veya yerin altındaysa 0,0,0'a atabilir.
                // Bu yüzden SpawnPoint'in yere (NavMesh'e) tam değdiğinden emin ol.
                agent.Warp(_spawnPoint.position); 
                
                // Ekstra güvenlik: Yolu sıfırla
                agent.ResetPath();
            }
            
            // 5. Canı Fulle (Eğer havuzdan eski/yaralı bir asker geldiyse)
            var health = unit.GetComponent<Health>();
            if (health != null) health.ResetHealth();
        }
    }
}