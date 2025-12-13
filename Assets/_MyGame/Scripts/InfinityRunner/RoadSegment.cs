using UnityEngine;
using System.Collections.Generic;

namespace IndianOceanAssets.Engine2_5D.World
{
    // --- YENİ YAPI: GRUP ID'Lİ SLOT ---
    [System.Serializable]
    public class ObstacleSlot
    {
        public SimpleObstacle ObstacleRef;

        [Tooltip("0 = Sabit Durur.\n1 = Grup 1 ile karışır.\n2 = Grup 2 ile karışır...")]
        public int ShuffleGroupId = 0; // Varsayılan 0 (Sabit)
    }

    [System.Serializable]
    public class ObstacleRow
    {
        public string RowName = "Row";
        public List<ObstacleSlot> Columns = new List<ObstacleSlot>();
    }

    public class RoadSegment : MonoBehaviour
    {
        [SerializeField] private Transform _connectPoint;
        [SerializeField] private List<ObstacleRow> _obstacleRows = new List<ObstacleRow>();

        // Grupları ayırmak için Sözlük kullanıyoruz.
        // Anahtar (int): Grup Numarası (1, 2, 3...)
        // Değer (List): O gruba ait engeller
        private Dictionary<int, List<SimpleObstacle>> _shuffleGroups = new Dictionary<int, List<SimpleObstacle>>();

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
            // 1. Grupları Analiz Et ve Ayır
            IdentifyShuffleGroups();

            // 2. Her grubu kendi içinde karıştır
            foreach (var groupID in _shuffleGroups.Keys)
            {
                ShuffleSpecificGroup(_shuffleGroups[groupID]);
            }

            // 3. Hepsini Görünür Yap
            foreach (var row in _obstacleRows)
            {
                foreach (var slot in row.Columns)
                {
                    if (slot.ObstacleRef != null) 
                        slot.ObstacleRef.gameObject.SetActive(true);
                }
            }
        }

        // --- GRUPLAMA MANTIĞI ---
        private void IdentifyShuffleGroups()
        {
            // Sözlüğü temizle
            foreach (var list in _shuffleGroups.Values) list.Clear();
            _shuffleGroups.Clear();

            foreach (var row in _obstacleRows)
            {
                foreach (var slot in row.Columns)
                {
                    // Eğer ID 0'dan büyükse (yani bir gruba dahilse) ve obje varsa
                    if (slot.ObstacleRef != null && slot.ShuffleGroupId > 0)
                    {
                        // Bu Grup ID'si sözlükte yoksa, yeni liste oluştur
                        if (!_shuffleGroups.ContainsKey(slot.ShuffleGroupId))
                        {
                            _shuffleGroups[slot.ShuffleGroupId] = new List<SimpleObstacle>();
                        }

                        // Objeyi ilgili grubun listesine ekle
                        _shuffleGroups[slot.ShuffleGroupId].Add(slot.ObstacleRef);
                    }
                }
            }
        }

        // --- GRUP BAZLI KARIŞTIRMA ---
        private void ShuffleSpecificGroup(List<SimpleObstacle> groupMembers)
        {
            // Eğer grupta 1 veya daha az kişi varsa karıştırmaya gerek yok
            if (groupMembers.Count <= 1) return;

            // A. Orijinal pozisyonları topla
            List<Vector3> positions = new List<Vector3>();
            foreach (var obs in groupMembers)
            {
                positions.Add(obs.OriginalLocalPosition); 
            }

            // B. Pozisyonları karıştır (Fisher-Yates)
            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 temp = positions[i];
                int randomIndex = Random.Range(i, positions.Count);
                positions[i] = positions[randomIndex];
                positions[randomIndex] = temp;
            }

            // C. Yeni pozisyonları uygula
            for (int i = 0; i < groupMembers.Count; i++)
            {
                groupMembers[i].transform.localPosition = positions[i];
            }
        }

        // --- GRID BAKE (AYNI KALDI, SADECE SLOT GÜNCELLENDİ) ---
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
            
            Debug.Log($"✅ Grid Güncellendi! 'Shuffle Group Id' ile gruplama yapabilirsin.");
        }

        private void AddRowToGrid(List<SimpleObstacle> unsortedRow)
        {
            unsortedRow.Sort((a, b) => a.transform.localPosition.x.CompareTo(b.transform.localPosition.x));
            
            ObstacleRow newRow = new ObstacleRow();
            newRow.RowName = $"Row {_obstacleRows.Count}";
            
            foreach(var obs in unsortedRow)
            {
                ObstacleSlot newSlot = new ObstacleSlot();
                newSlot.ObstacleRef = obs;
                newSlot.ShuffleGroupId = 0; // Varsayılan 0 (Sabit)
                newRow.Columns.Add(newSlot);
            }
            
            _obstacleRows.Add(newRow);
        }
    }
}