using UnityEngine;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D.World;

namespace IndianOceanAssets.Engine2_5D.Managers
{
    public class RoadManager : MonoBehaviour
    {
        [Header("🛣️ Yol Ayarları")]
        [SerializeField] private RoadSegment _roadPrefab;
        [SerializeField] private int _poolSize = 7;

        [Header("🔗 Bağlantılar")]
        [SerializeField] private Transform _dollyTransform;

        private List<RoadSegment> _activeSegments = new List<RoadSegment>();
        private Transform _transform;

        private void Start()
        {
            _transform = transform;
            InitializeRoad();
        }

        private void InitializeRoad()
        {
            Vector3 spawnPosition = Vector3.zero; 
            for (int i = 0; i < _poolSize; i++)
            {
                SpawnSegment(ref spawnPosition);
            }
        }

        private void Update()
        {
            if (_activeSegments.Count == 0 || _dollyTransform == null) return;

            RoadSegment firstSeg = _activeSegments[0];

            // +15f güvenlik payı (Kamera geçtikten biraz sonra silinsin)
            if (_dollyTransform.position.z > firstSeg.GetEndPosition().z + 15f)
            {
                RecycleSegment();
            }
        }

        private void SpawnSegment(ref Vector3 position)
        {
            RoadSegment newSeg = Instantiate(_roadPrefab, position, Quaternion.identity, _transform);
            _activeSegments.Add(newSeg);
            position = newSeg.GetEndPosition();
        }

        private void RecycleSegment()
        {
            RoadSegment segmentToMove = _activeSegments[0];
            _activeSegments.RemoveAt(0);

            RoadSegment lastSegment = _activeSegments[_activeSegments.Count - 1];
            segmentToMove.transform.position = lastSegment.GetEndPosition();
            
            // [YENİ EKLENEN SATIR]
            // Yol yeni yerine geçtiğinde üzerindeki küpleri aç.
            segmentToMove.ResetObstacles();

            _activeSegments.Add(segmentToMove);
        }
    }
}