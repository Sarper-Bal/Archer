using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D; // EnemyDefinition ve Controller için

namespace IndianOceanAssets.Engine2_5D.Spawners
{
    /// <summary>
    /// [TR] Verilen düşman türünü, belirlenen alanda ve sayıda doğuran basit spawner.
    /// [TR] Karmaşık Wave veya Budget sistemlerini kullanmaz.
    /// </summary>
    public class SimpleAreaSpawner : MonoBehaviour
    {
        [Header("📋 Ayarlar")]
        [Tooltip("Hangi düşman doğacak?")]
        public EnemyDefinition EnemyData;

        [Tooltip("Kaç adet doğacak? (-1 yaparsan sonsuza kadar doğar)")]
        public int SpawnCount = 10;

        [Tooltip("İki doğma arası bekleme süresi (saniye).")]
        public float SpawnInterval = 1.0f;

        [Header("📏 Alan Ayarları")]
        [Tooltip("Doğma alanının genişliği (X ve Z ekseni).")]
        public Vector3 SpawnAreaSize = new Vector3(10, 0, 10);

        // --- BASİT POOL (HAVUZ) SİSTEMİ ---
        // Optimizasyon için şart. Instantiate/Destroy yapmamak için.
        private Queue<EnemyBehaviorController> _pool = new Queue<EnemyBehaviorController>();

        private void Start()
        {
            if (EnemyData != null)
            {
                StartCoroutine(SpawnRoutine());
            }
            else
            {
                Debug.LogError("⚠️ SimpleAreaSpawner: EnemyData boş! Lütfen bir düşman ScriptableObject'i atayın.");
            }
        }

        private IEnumerator SpawnRoutine()
        {
            int spawnedSoFar = 0;

            // Sonsuz döngü (-1) veya sayıya ulaşana kadar devam et
            while (SpawnCount == -1 || spawnedSoFar < SpawnCount)
            {
                SpawnSingleEnemy();
                spawnedSoFar++;

                yield return new WaitForSeconds(SpawnInterval);
            }
        }

        private void SpawnSingleEnemy()
        {
            // 1. Havuzdan veya yeni üretimle objeyi al
            EnemyBehaviorController enemy = GetFromPool();
            if (enemy == null) return;

            // 2. Rastgele konum belirle (Objenin kendi konumu + Rastgele Sapma)
            Vector3 randomPos = GetRandomPosition();
            enemy.transform.position = randomPos;
            enemy.transform.rotation = Quaternion.identity; // Düz başlasın, Controller yönü halleder

            // 3. Düşmanı başlat (Bu metot önceki adımlarda optimize ettiğimiz metot)
            // Düşman türünü (Hız, Can, Model vb.) yükler.
            enemy.InitializeEnemy(EnemyData);
        }

        /// <summary>
        /// Rastgele bir nokta seçer.
        /// </summary>
        private Vector3 GetRandomPosition()
        {
            float x = Random.Range(-SpawnAreaSize.x / 2, SpawnAreaSize.x / 2);
            float z = Random.Range(-SpawnAreaSize.z / 2, SpawnAreaSize.z / 2);

            // Spawner'ın dünya pozisyonuna ekle
            return transform.position + new Vector3(x, 0, z);
        }

        /// <summary>
        /// Basit Havuz Mantığı: Varsa eskisini ver, yoksa yenisini üret.
        /// </summary>
        private EnemyBehaviorController GetFromPool()
        {
            // Havuzda bekleyen var mı?
            if (_pool.Count > 0)
            {
                EnemyBehaviorController pooled = _pool.Dequeue();
                if (pooled != null)
                {
                    pooled.OnReturnToPool = ReturnToPool; // Geri dönüş biletini tazele
                    return pooled;
                }
            }

            // Yoksa ve Prefab varsa yeni üret
            if (EnemyData.EnemyPrefab != null)
            {
                GameObject newObj = Instantiate(EnemyData.EnemyPrefab, transform);
                var controller = newObj.GetComponent<EnemyBehaviorController>();
                
                // Önemli: Düşman ölünce (OnDisable) bu metoda dönsün
                controller.OnReturnToPool = ReturnToPool;
                
                newObj.SetActive(false);
                return controller;
            }

            return null;
        }

        /// <summary>
        /// Düşman öldüğünde buraya geri döner.
        /// </summary>
        private void ReturnToPool(EnemyBehaviorController enemy)
        {
            if (enemy != null)
            {
                enemy.gameObject.SetActive(false);
                _pool.Enqueue(enemy);
            }
        }

        // Editörde alanı çizmek için (Yeşil Kutu)
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f); // Yarı saydam yeşil
            Gizmos.DrawCube(transform.position, new Vector3(SpawnAreaSize.x, 0.2f, SpawnAreaSize.z));
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(SpawnAreaSize.x, 0.2f, SpawnAreaSize.z));
        }
    }
}