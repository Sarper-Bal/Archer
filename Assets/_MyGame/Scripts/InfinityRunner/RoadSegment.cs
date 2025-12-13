using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Sıralama işlemleri için gerekli

namespace IndianOceanAssets.Engine2_5D.World
{
    // --- YARDIMCI SINIF: SATIR ---
    [System.Serializable] // Inspector'da görünmesi için şart
    public class ObstacleRow
    {
        public string RowName = "Row"; // Debug için isim
        public List<SimpleObstacle> Columns = new List<SimpleObstacle>();
    }

    // --- ANA SINIF ---
    public class RoadSegment : MonoBehaviour
    {
        [Tooltip("Yolun bittiği nokta.")]
        [SerializeField] private Transform _connectPoint;

        [Header("🧩 Akıllı Grid Sistemi")]
        [Tooltip("Yol üzerindeki engellerin satır satır listesi.")]
        [SerializeField] private List<ObstacleRow> _obstacleRows = new List<ObstacleRow>();

        private void Awake()
        {
            // Eğer liste boşsa, oyun başında bir kereye mahsus otomatik tara
            if (_obstacleRows == null || _obstacleRows.Count == 0)
            {
                BakeObstaclesToGrid();
            }
        }

        public Vector3 GetEndPosition()
        {
            if (_connectPoint == null)
                return transform.position + Vector3.forward * 20f;
            return _connectPoint.position;
        }

        public void ResetObstacles()
        {
            // Grid içindeki tüm satırları ve sütunları gez
            foreach (var row in _obstacleRows)
            {
                foreach (var obstacle in row.Columns)
                {
                    if (obstacle != null)
                    {
                        obstacle.gameObject.SetActive(true);
                        // İleride buraya mantık ekleyeceğiz:
                        // if (row.Index == 2) obstacle.Zıpla();
                    }
                }
            }
        }

        // --- EDİTÖR İÇİN AKILLI SIRALAMA ALGORİTMASI ---
        // Bu kod, dağınık duran küpleri Z (İleri) ve X (Yan) pozisyonlarına göre gruplar.
        [ContextMenu("⚡ Grid Sistemini Oluştur (Bake)")]
        private void BakeObstaclesToGrid()
        {
            _obstacleRows.Clear();

            // 1. Tüm çocuk engelleri bul
            List<SimpleObstacle> allObstacles = new List<SimpleObstacle>();
            GetComponentsInChildren(true, allObstacles);

            if (allObstacles.Count == 0)
            {
                Debug.LogWarning("⚠️ Hiç engel (SimpleObstacle) bulunamadı!");
                return;
            }

            // 2. Z Pozisyonuna (Derinlik) göre sırala (Yakından uzağa)
            // Böylece Row 0 her zaman en yakındaki olur.
            allObstacles.Sort((a, b) => a.transform.localPosition.z.CompareTo(b.transform.localPosition.z));

            // 3. Gruplama (Z pozisyonları birbirine çok yakın olanları aynı satıra koy)
            float rowThreshold = 0.5f; // Yarım metre hata payı bırakıyoruz
            List<SimpleObstacle> currentRowList = new List<SimpleObstacle>();
            float lastZ = allObstacles[0].transform.localPosition.z;

            foreach (var obs in allObstacles)
            {
                // Eğer bu engelin Z'si, bir öncekinden çok farklıysa -> Yeni Satıra geç
                if (Mathf.Abs(obs.transform.localPosition.z - lastZ) > rowThreshold)
                {
                    AddRowToGrid(currentRowList); // Önceki satırı kaydet
                    currentRowList = new List<SimpleObstacle>(); // Yeni liste aç
                    lastZ = obs.transform.localPosition.z; // Referansı güncelle
                }
                
                currentRowList.Add(obs);
            }
            // Son kalan grubu da ekle
            if (currentRowList.Count > 0) AddRowToGrid(currentRowList);

            Debug.Log($"✅ Grid Oluşturuldu: {_obstacleRows.Count} Satır bulundu.");
        }

        // Yardımcı fonksiyon: Bir satırı kaydetmeden önce X'e göre (Soldan Sağa) sıralar
        private void AddRowToGrid(List<SimpleObstacle> unsortedRow)
        {
            // Soldan Sağa sırala (X değeri küçükten büyüğe)
            unsortedRow.Sort((a, b) => a.transform.localPosition.x.CompareTo(b.transform.localPosition.x));

            ObstacleRow newRow = new ObstacleRow();
            newRow.RowName = $"Row {_obstacleRows.Count}"; // İsim ver (Row 0, Row 1...)
            newRow.Columns = new List<SimpleObstacle>(unsortedRow);
            
            _obstacleRows.Add(newRow);
        }

        private void OnDrawGizmos()
        {
            if (_connectPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(_connectPoint.position, 0.5f);
            }
        }
    }
}