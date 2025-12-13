using UnityEngine;
using System.Collections.Generic;

namespace IndianOceanAssets.Engine2_5D.World
{
    [System.Serializable]
    public class ObstacleRow
    {
        public string RowName = "Row";
        public List<SimpleObstacle> Columns = new List<SimpleObstacle>();
    }

    public class RoadSegment : MonoBehaviour
    {
        [SerializeField] private Transform _connectPoint;
        [SerializeField] private List<ObstacleRow> _obstacleRows = new List<ObstacleRow>();

        // Karıştırılacakların özel listesi (Optimizasyon için ayrı tutuyoruz)
        private List<SimpleObstacle> _shuffleableObstacles = new List<SimpleObstacle>();

        private void Awake()
        {
            if (_obstacleRows == null || _obstacleRows.Count == 0) BakeObstaclesToGrid();
            
            // Oyun başında karıştırılabilir olanları bul ve listeye al
            CacheShuffleableObstacles();
        }

        private void CacheShuffleableObstacles()
        {
            _shuffleableObstacles.Clear();
            // Tüm gridi gez, 'AllowShuffle' olanları bul
            foreach (var row in _obstacleRows)
            {
                foreach (var obs in row.Columns)
                {
                    if (obs != null && obs.AllowShuffle)
                    {
                        _shuffleableObstacles.Add(obs);
                    }
                }
            }
        }

        public Vector3 GetEndPosition()
        {
            if (_connectPoint == null) return transform.position + Vector3.forward * 20f;
            return _connectPoint.position;
        }

        public void ResetObstacles()
        {
            // 1. Önce Karıştırma İşlemini Yap (Eğer listede eleman varsa)
            if (_shuffleableObstacles.Count > 1)
            {
                ShufflePositions();
            }

            // 2. Sonra Hepsini Aç
            foreach (var row in _obstacleRows)
            {
                foreach (var obstacle in row.Columns)
                {
                    if (obstacle != null) obstacle.gameObject.SetActive(true);
                }
            }
        }

        // --- POZİSYON KARIŞTIRMA ALGORİTMASI ---
        private void ShufflePositions()
        {
            // A. Mevcut pozisyonların kopyasını al
            List<Vector3> positions = new List<Vector3>();
            foreach (var obs in _shuffleableObstacles)
            {
                positions.Add(obs.transform.localPosition); // Yerel pozisyonu al
            }

            // B. Pozisyon listesini karıştır (Fisher-Yates Algoritması)
            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 temp = positions[i];
                int randomIndex = Random.Range(i, positions.Count);
                positions[i] = positions[randomIndex];
                positions[randomIndex] = temp;
            }

            // C. Karışmış pozisyonları küplere geri yükle
            for (int i = 0; i < _shuffleableObstacles.Count; i++)
            {
                _shuffleableObstacles[i].transform.localPosition = positions[i];
            }
        }

        // --- GRID BAKE KODLARI (Öncekiyle Aynı) ---
        [ContextMenu("⚡ Grid Sistemini Oluştur (Bake)")]
        private void BakeObstaclesToGrid()
        {
            _obstacleRows.Clear();
            List<SimpleObstacle> allObstacles = new List<SimpleObstacle>();
            GetComponentsInChildren(true, allObstacles);
            if (allObstacles.Count == 0) return;

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
            
            // Grid oluştuktan sonra shuffle listesini de güncelle (Editörde görmek istersen)
            CacheShuffleableObstacles();
        }

        private void AddRowToGrid(List<SimpleObstacle> unsortedRow)
        {
            unsortedRow.Sort((a, b) => a.transform.localPosition.x.CompareTo(b.transform.localPosition.x));
            ObstacleRow newRow = new ObstacleRow();
            newRow.RowName = $"Row {_obstacleRows.Count}";
            newRow.Columns = new List<SimpleObstacle>(unsortedRow);
            _obstacleRows.Add(newRow);
        }
    }
}