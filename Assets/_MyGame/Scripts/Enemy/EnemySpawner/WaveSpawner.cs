using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D; 
using IndianOceanAssets.Engine2_5D.Managers; // SmartWaveManager için
using ArcadeBridge.ArcadeIdleEngine.Enemy;

namespace ArcadeBridge.ArcadeIdleEngine.Spawners
{
    public class WaveSpawner : MonoBehaviour
    {
        [Header("🧠 AI Bağlantısı")]
        [SerializeField] private SmartWaveManager _director;

        [Header("⏱️ Dalga Ayarları")]
        [Tooltip("Bir dalganın spawn olma süresi (Saniye). Düşmanlar bu süreye yayılır.")]
        [SerializeField] private float _waveDuration = 60f;
        
        [Tooltip("İki dalga arasındaki dinlenme süresi.")]
        [SerializeField] private float _timeBetweenWaves = 5f;

        [Header("📍 Alan Ayarları")]
        [SerializeField] private Vector3 _spawnAreaSize = new Vector3(10, 0, 10);
        
        // [YENİ SİSTEM] Multi-Pool: Her düşman tipi (Definition Adı) için ayrı bir kuyruk tutar.
        // string: Düşman Türü (Örn: "Slime"), Queue: O türün yedekleri
        private Dictionary<string, Queue<EnemyBehaviorController>> _poolDictionary = new Dictionary<string, Queue<EnemyBehaviorController>>();
        
        // Aktif düşmanları takip listesi
        private List<EnemyBehaviorController> _activeEnemies = new List<EnemyBehaviorController>();

        // Cache
        private WaitForSeconds _checkInterval = new WaitForSeconds(1f); 
        private bool _isWaveActive = false;

        public System.Action<int> OnWaveStarted; // UI için event
        public System.Action OnWaveCleared;

        private void Start()
        {
            // Oyuna başlamadan önce referans kontrolü
            if (_director == null)
            {
                Debug.LogError("⚠️ WaveSpawner: SmartWaveManager (Director) atanmamış! Lütfen Inspector'dan atayın.");
                return;
            }

            StartCoroutine(GameLoopRoutine());
        }

        private IEnumerator GameLoopRoutine()
        {
            // Sonsuz Oyun Döngüsü
            while (true)
            {
                // 1. HAZIRLIK: AI'dan yeni dalgayı iste
                _director.GenerateNextWave(); 
                List<EnemyDefinition> enemiesToSpawn = _director.NextWaveEnemies;
                
                if (enemiesToSpawn.Count == 0)
                {
                    Debug.LogWarning("⚠️ AI Director boş liste gönderdi. Bütçe yetersiz olabilir.");
                    yield return new WaitForSeconds(2f);
                    continue; // Tekrar dene
                }

                OnWaveStarted?.Invoke(enemiesToSpawn.Count);
                _isWaveActive = true;

                // 2. SAVAŞ: Düşmanları zamana yayarak spawn et
                // Formül: Eğer 60 saniyemiz ve 60 düşmanımız varsa, her 1 saniyede bir düşman çıkar.
                float spawnDelay = _waveDuration / (float)enemiesToSpawn.Count;
                WaitForSeconds waitDelay = new WaitForSeconds(spawnDelay);

                foreach (EnemyDefinition enemyData in enemiesToSpawn)
                {
                    SpawnEnemy(enemyData);
                    yield return waitDelay; // Sıradaki düşman için bekle
                }

                // 3. BEKLEME: Hepsi ölene kadar bekle (Hepsini Öldür Modu)
                Debug.Log("⏳ Spawn bitti, temizlik bekleniyor...");
                
                while (_activeEnemies.Count > 0)
                {
                    yield return _checkInterval; 
                }

                // 4. ZAFER: Dalga bitti, AI'ya haber ver (Zorluğu artırsın)
                _isWaveActive = false;
                _director.OnWaveWon(); 
                OnWaveCleared?.Invoke();

                Debug.Log($"🎉 Dalga Temizlendi! {_timeBetweenWaves} saniye mola...");
                yield return new WaitForSeconds(_timeBetweenWaves);
            }
        }

        // --- SPAWN SİSTEMİ (Multi-Pool Logic) ---

        private void SpawnEnemy(EnemyDefinition data)
        {
            // Hangi prefab? (Resources'dan yüklemek yerine direkt veriden alıyoruz)
            // NOT: EnemyDefinition scriptine "Prefab" değişkeni eklememiz gerekebilir, 
            // ya da verinin adı ile Resources.Load yapabiliriz. 
            // Şimdilik verinin adını anahtar olarak kullanıyoruz.
            
            // Havuzdan çek veya yeni yarat
            EnemyBehaviorController enemy = GetFromPool(data);

            // Pozisyonla (Hala kapalı)
            Vector3 randomPos = GetRandomPosition();
            enemy.transform.position = randomPos;
            enemy.transform.rotation = Quaternion.identity;

            // [ÖNEMLİ] İstatistiklerini Yükle (Stat Scriptini bul ve datayı ver)
            // Bu kısım çok kritik, yoksa bütün düşmanlar aynı güçte olur.
            var stats = enemy.GetComponent<EnemyStats>();
            if (stats != null)
            {
                // Reflection veya stat scriptinde public bir "SetData" metodu olması lazım.
                // Şimdilik EnemyStats scriptinde "EnemyDefinition" serialized field olduğu için
                // onu runtime'da değiştirmek gerekebilir. 
                // *Bunun için EnemyStats scriptine minik bir ekleme yapacağız.*
                stats.InitializeRuntime(data);            }

            // Eğer devriye rotası varsa ata
            if (data.DefaultBehavior == EnemyBehaviorType.Patrol && data.PatrolRouteID != null)
            {
                // RouteManager entegrasyonu (varsa)
            }

            // Aktifleştir
            enemy.gameObject.SetActive(true);
            _activeEnemies.Add(enemy);
        }

        private EnemyBehaviorController GetFromPool(EnemyDefinition data)
        {
            // Prefab ismini anahtar olarak kullan (Örn: "Goblin_Data")
            string key = data.name; 

            // 1. Bu tür için bir rafımız var mı? Yoksa oluştur.
            if (!_poolDictionary.ContainsKey(key))
            {
                _poolDictionary[key] = new Queue<EnemyBehaviorController>();
            }

            // 2. Rafta hazır asker var mı?
            if (_poolDictionary[key].Count > 0)
            {
                EnemyBehaviorController pooledEnemy = _poolDictionary[key].Dequeue();
                
                // [GÜVENLİK] Obje sahnede silinmişse (Destroy olduysa) yenisini yarat
                if (pooledEnemy != null) 
                {
                    pooledEnemy.OnReturnToPool = ReturnEnemyToPool; // Bileti tazele
                    return pooledEnemy;
                }
            }

            // 3. Yoksa yeni üret (Instantiate)
            // EnemyDefinition içinde Prefab referansı tutmadığımız için (henüz),
            // şimdilik "Default" bir düşman prefabı kullanmak zorundayız veya 
            // EnemyDefinition'a "EnemyBehaviorController Prefab" eklemeliyiz.
            // *ÇÖZÜM:* Geçici olarak Resources.Load kullanıyoruz, ama doğrusu Definiton'a eklemektir.
            
            // VARSAYIM: Düşman prefabının adı, Data dosyasının adıyla aynı (Örn: "Slime")
            GameObject prefab = Resources.Load<GameObject>("Enemies/" + data.name);
            
            if (prefab == null)
            {
                Debug.LogError($"❌ PREFAB BULUNAMADI: 'Resources/Enemies/{data.name}' yolunda prefab yok! Lütfen kontrol et.");
                return null;
            }

            GameObject newObj = Instantiate(prefab, transform);
            var controller = newObj.GetComponent<EnemyBehaviorController>();
            
            // Eve dönüş bileti ver
            controller.OnReturnToPool = ReturnEnemyToPool;
            newObj.SetActive(false); // Kapalı başlat
            
            return controller;
        }

        private void ReturnEnemyToPool(EnemyBehaviorController enemy)
        {
            if (this == null) return;

            // Listeden sil
            if (_activeEnemies.Contains(enemy)) _activeEnemies.Remove(enemy);

            enemy.gameObject.SetActive(false);

            // Hangi rafa koyacağız?
            // Düşmanın üzerindeki Stat scriptinden kimliğini (Definition) al
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
                Destroy(enemy.gameObject); // Kimliği yoksa yok et (Çöp olmasın)
            }
        }

        private Vector3 GetRandomPosition()
        {
            float x = Random.Range(-_spawnAreaSize.x / 2, _spawnAreaSize.x / 2);
            float z = Random.Range(-_spawnAreaSize.z / 2, _spawnAreaSize.z / 2);
            return transform.position + new Vector3(x, 0, z);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _isWaveActive ? Color.red : Color.green;
            Gizmos.DrawWireCube(transform.position, _spawnAreaSize);
        }
    }
}