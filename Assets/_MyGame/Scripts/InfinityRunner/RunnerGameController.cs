using UnityEngine;

namespace IndianOceanAssets.Engine2_5D.Managers
{
    public class RunnerGameController : MonoBehaviour
    {
        [Header("🚄 Koşu Ayarları")]
        [Tooltip("Oyunun akış hızı.")]
        [SerializeField] private float _scrollSpeed = 5.0f;

        [Header("↔️ Yan Sınır Ayarları")]
        [Tooltip("Oyuncu merkezden sağa/sola en fazla kaç birim gidebilir?")]
        [SerializeField] private float _xBoundLimit = 4.5f;

        [Header("↕️ Dikey Sınır Ayarları")]
        [Tooltip("Oyuncu Lokomotifin (Kameranın) ne kadar gerisinde kalabilir?")]
        [SerializeField] private float _maxLagDistance = 10.0f; // Varsayılanı artırdım ki başlangıçta hemen çekmesin

        [Tooltip("İleriye gidişi sınırlayalım mı?")]
        [SerializeField] private bool _limitForwardMovement = true;

        [Tooltip("Eğer sınır açıksa: Oyuncu Lokomotifin ne kadar önüne geçebilir?")]
        [SerializeField] private float _maxForwardDistance = 6.0f;

        [Header("🔗 Zorunlu Bağlantılar")]
        [SerializeField] private Transform _dollyTransform;
        [SerializeField] private Transform _playerTransform;

        private void Start()
        {
            InitializeSystem();
        }

        private void InitializeSystem()
        {
            // 1. OYUNCUYU BUL
            if (_playerTransform == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    _playerTransform = playerObj.transform;
                }
                else
                {
                    Debug.LogError("❌ HATA: 'Player' etiketli obje bulunamadı!");
                    return; 
                }
            }

            // 2. DOLLY KONTROLÜ
            if (_dollyTransform == null)
            {
                Debug.LogError("❌ Hata: Runner_Dolly atanmamış!");
                // Acil durum: Dolly yoksa mecburen oyuncunun olduğu yerde yarat
                GameObject tempDolly = new GameObject("Temp_Dolly");
                if (_playerTransform != null) tempDolly.transform.position = _playerTransform.position;
                _dollyTransform = tempDolly.transform;
            }
            
            // [DEĞİŞİKLİK BURADA]
            // Eskiden burada "_dollyTransform.position = _playerTransform.position" yazıyordu.
            // O satırı SİLDİM. Artık Dolly, sen sahneye nereye koyduysan oradan başlar.
            // Böylece oyuncuyu kameranın altına (gerisine) koyduğun ayar bozulmaz.
        }

        private void Update()
        {
            if (_dollyTransform == null) return;

            // A. LOKOMOTİFİ İLERLET
            _dollyTransform.Translate(Vector3.forward * _scrollSpeed * Time.deltaTime);
        }

        private void LateUpdate()
        {
            if (_playerTransform == null || _dollyTransform == null) return;

            Vector3 playerPos = _playerTransform.position;
            Vector3 dollyPos = _dollyTransform.position;

            // --- 1. SAĞ / SOL SINIRI ---
            float minX = dollyPos.x - _xBoundLimit;
            float maxX = dollyPos.x + _xBoundLimit;
            playerPos.x = Mathf.Clamp(playerPos.x, minX, maxX);

            // --- 2. GERİ VE İLERİ SINIRI ---
            
            float minZ = dollyPos.z - _maxLagDistance;
            float maxZ = _limitForwardMovement ? (dollyPos.z + _maxForwardDistance) : Mathf.Infinity;

            playerPos.z = Mathf.Clamp(playerPos.z, minZ, maxZ);

            _playerTransform.position = playerPos;
        }
    }
}