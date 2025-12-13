using UnityEngine;
using System.Collections.Generic;
using IndianOceanAssets.Engine2_5D.World; // RoadSegment'i bulmak için

namespace IndianOceanAssets.Engine2_5D.Managers
{
    public class RoadManager : MonoBehaviour
    {
        [Header("🛣️ Yol Ayarları")]
        [Tooltip("Oluşturulacak yol parçası prefab'ı.")]
        [SerializeField] private RoadSegment _roadPrefab;

        [Tooltip("Sahneye kaç tane yol parçası dizilsin? (5-7 arası idealdir).")]
        [SerializeField] private int _poolSize = 7;

        [Header("🔗 Bağlantılar")]
        [Tooltip("Kameranın takip ettiği Dolly (Lokomotif) objesi.")]
        [SerializeField] private Transform _dollyTransform;

        // Havuzdaki yolları tutan liste (Ring Buffer)
        private List<RoadSegment> _activeSegments = new List<RoadSegment>();

        // Optimizasyon için önbellek
        private Transform _transform;

        private void Start()
        {
            _transform = transform;
            InitializeRoad();
        }

        private void InitializeRoad()
        {
            Vector3 spawnPosition = Vector3.zero; // İlk parça (0,0,0) noktasında başlar

            for (int i = 0; i < _poolSize; i++)
            {
                SpawnSegment(ref spawnPosition);
            }
        }

        private void Update()
        {
            if (_activeSegments.Count == 0 || _dollyTransform == null) return;

            // En arkadaki (Listenin başındaki) parçayı kontrol et
            RoadSegment firstSeg = _activeSegments[0];

            // Eğer Dolly, bu parçanın bitiş noktasını çoktan geçtiyse...
            // (Parça kameranın arkasında kaldıysa)
            // Not: +10f güvenlik payıdır, kamera görürken silinmesin diye.
            if (_dollyTransform.position.z > firstSeg.GetEndPosition().z + 10f)
            {
                RecycleSegment();
            }
        }

        private void SpawnSegment(ref Vector3 position)
        {
            // Yeni oluştur (Sadece oyun başında çalışır)
            RoadSegment newSeg = Instantiate(_roadPrefab, position, Quaternion.identity, _transform);
            
            // Listeye ekle
            _activeSegments.Add(newSeg);

            // Bir sonraki parçanın başlangıç pozisyonunu güncelle
            position = newSeg.GetEndPosition();
        }

        private void RecycleSegment()
        {
            // 1. En arkadaki parçayı al
            RoadSegment segmentToMove = _activeSegments[0];
            _activeSegments.RemoveAt(0);

            // 2. Şu anki en öndeki parçayı bul (Eskiden sonuncuydu)
            RoadSegment lastSegment = _activeSegments[_activeSegments.Count - 1];

            // 3. Arkadaki parçayı, öndekinin ucuna ışınla
            segmentToMove.transform.position = lastSegment.GetEndPosition();

            // 4. Parçayı listenin sonuna ekle (Artık en yeni parça o)
            _activeSegments.Add(segmentToMove);
        }
    }
}