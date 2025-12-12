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
        [SerializeField] private float _maxLagDistance = 6.0f;

        [Tooltip("İleriye gidişi sınırlayalım mı? (Kutuyu işaretlersen oyuncu kamerayı geçemez)")]
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
                GameObject tempDolly = new GameObject("Temp_Dolly");
                if (_playerTransform != null) tempDolly.transform.position = _playerTransform.position;
                _dollyTransform = tempDolly.transform;
            }
            else
            {
                if (_playerTransform != null)
                {
                    Vector3 startPos = _playerTransform.position;
                    _dollyTransform.position = new Vector3(startPos.x, startPos.y, startPos.z);
                }
            }
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

            // --- 1. SAĞ / SOL SINIRI (X Ekseni) ---
            float minX = dollyPos.x - _xBoundLimit;
            float maxX = dollyPos.x + _xBoundLimit;
            playerPos.x = Mathf.Clamp(playerPos.x, minX, maxX);

            // --- 2. GERİ VE İLERİ SINIRI (Z Ekseni) ---
            
            // En geri gidebileceği nokta (Kamera alt sınırı)
            float minZ = dollyPos.z - _maxLagDistance;

            // En ileri gidebileceği nokta (Kamera üst sınırı)
            // Eğer sınırlama kapalıysa (+Sonsuz), açıksa (_maxForwardDistance) kullan.
            float maxZ = _limitForwardMovement ? (dollyPos.z + _maxForwardDistance) : Mathf.Infinity;

            // Oyuncuyu bu iki Z değeri arasına hapsediyoruz (Kelepçeleme)
            playerPos.z = Mathf.Clamp(playerPos.z, minZ, maxZ);

            // Pozisyonu uygula
            _playerTransform.position = playerPos;
        }
    }
}