using UnityEngine;
using System.Collections.Generic;

namespace IndianOceanAssets.Engine2_5D.World
{
    // --- YENİ YAPI: TEKİL ENGEL KUTUSU ---
    [System.Serializable]
    public class ObstacleSlot
    {
        [Tooltip("Sahnedeki Engel Objesi")]
        public SimpleObstacle ObstacleRef;

        [Tooltip("✅ Eğer bunu işaretlersen, BU KÜP karıştırma havuzuna girer.")]
        public bool IsShuffleable = false;
    }

    // --- SATIR YAPISI ---
    [System.Serializable]
    public class ObstacleRow
    {
        public string RowName = "Row";
        // Artık direkt engel değil, 'Slot' listesi tutuyoruz
        public List<ObstacleSlot> Columns = new List<ObstacleSlot>();
    }

    // --- ANA SINIF ---
    public class RoadSegment : MonoBehaviour
    {
        [SerializeField] private Transform _connectPoint;
        [SerializeField] private List<ObstacleRow> _obstacleRows = new List<ObstacleRow>();

        private List<SimpleObstacle> _shuffleableObstacles = new List<SimpleObstacle>();

        private void Awake()
        {
            if (_obstacleRows == null || _obstacleRows.Count == 0) BakeObstaclesToGrid();
        }

        private void Start()
        {
            ResetObstacles();
        }

        public Vector3 GetEndPosition()
        {
            if (_connectPoint == null) return transform.position + Vector3.forward * 20f;
            return _connectPoint.position;
        }

        public void ResetObstacles()
        {
            // 1. Karıştırılacakları Bul
            IdentifyShuffleTargets();

            // 2. Karıştır
            if (_shuffleableObstacles.Count > 1)
            {
                ShufflePositions();
            }

            // 3. Hepsini Aç
            foreach (var row in _obstacleRows)
            {
                foreach (var slot in row.Columns)
                {
                    if (slot.ObstacleRef != null) 
                        slot.ObstacleRef.gameObject.SetActive(true);
                }
            }
        }

        // --- MİKRO KONTROL SEÇİMİ ---
        private void IdentifyShuffleTargets()
        {
            _shuffleableObstacles.Clear();

            foreach (var row in _obstacleRows)
            {
                foreach (var slot in row.Columns)
                {
                    // Artık satıra değil, TEK TEK slotlara bakıyoruz.
                    // Eğer sen o küpün yanındaki kutuyu işaretlediysen havuza girer.
                    if (slot.ObstacleRef != null && slot.IsShuffleable)
                    {
                        _shuffleableObstacles.Add(slot.ObstacleRef);
                    }
                }
            }
        }

        private void ShufflePositions()
        {
            List<Vector3> positions = new List<Vector3>();
            foreach (var obs in _shuffleableObstacles)
            {
                positions.Add(obs.OriginalLocalPosition); 
            }

            // Fisher-Yates Shuffle
            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 temp = positions[i];
                int randomIndex = Random.Range(i, positions.Count);
                positions[i] = positions[randomIndex];
                positions[randomIndex] = temp;
            }

            for (int i = 0; i < _shuffleableObstacles.Count; i++)
            {
                _shuffleableObstacles[i].transform.localPosition = positions[i];
            }
        }

        // --- GRID BAKE (GÜNCELLENDİ) ---
        [ContextMenu("⚡ Grid Sistemini Oluştur (Bake)")]
        private void BakeObstaclesToGrid()
        {
            _obstacleRows.Clear();
            List<SimpleObstacle> allObstacles = new List<SimpleObstacle>();
            GetComponentsInChildren(true, allObstacles);
            if (allObstacles.Count == 0) return;

            // Z'ye göre sırala
            allObstacles.Sort((a, b) => a.transform.localPosition.z.CompareTo(b.transform.localPosition.z));

            float rowThreshold = 0.5f;
            List<SimpleObstacle> currentRowList = new List<SimpleObstacle>();
            float lastZ = allObstacles[0].transform.localPosition.z;

            foreach (var obs in allObstacles)
            {
                if (Mathf.Abs(obs.transform.localPosition.z - lastZ) > rowThreshold)
                {
                    AddRowToGrid(currentRowList);
                    currentRowList = new List<SimpleObstacle>();
                    lastZ = obs.transform.localPosition.z;
                }
                currentRowList.Add(obs);
            }
            if (currentRowList.Count > 0) AddRowToGrid(currentRowList);
            
            Debug.Log($"✅ Grid Güncellendi! Artık her küpü tek tek seçebilirsin.");
        }

        private void AddRowToGrid(List<SimpleObstacle> unsortedRow)
        {
            unsortedRow.Sort((a, b) => a.transform.localPosition.x.CompareTo(b.transform.localPosition.x));
            
            ObstacleRow newRow = new ObstacleRow();
            newRow.RowName = $"Row {_obstacleRows.Count}";
            
            // Listeyi 'ObstacleSlot' yapısına çevirerek ekle
            foreach(var obs in unsortedRow)
            {
                ObstacleSlot newSlot = new ObstacleSlot();
                newSlot.ObstacleRef = obs;
                newSlot.IsShuffleable = false; // Varsayılan kapalı
                newRow.Columns.Add(newSlot);
            }
            
            _obstacleRows.Add(newRow);
        }
    }
}