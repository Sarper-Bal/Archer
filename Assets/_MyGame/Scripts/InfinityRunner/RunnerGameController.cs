using UnityEngine;

namespace IndianOceanAssets.Engine2_5D.Managers
{
    public class RunnerGameController : MonoBehaviour
    {
        [Header("🚄 Koşu Ayarları")]
        [Tooltip("Oyunun akış hızı.")]
        [SerializeField] private float _scrollSpeed = 5.0f;

        [Header("🛡️ Sınır Ayarları")]
        [Tooltip("Oyuncu merkezden sağa/sola en fazla kaç birim gidebilir?")]
        [SerializeField] private float _xBoundLimit = 4.5f;

        [Tooltip("Oyuncu Lokomotifin ne kadar gerisinde kalabilir?")]
        [SerializeField] private float _maxLagDistance = 6.0f;

        [Header("🔗 Zorunlu Bağlantılar")]
        [Tooltip("Sahnede oluşturduğun boş 'Runner_Dolly' objesini buraya sürükle.")]
        [SerializeField] private Transform _dollyTransform;

        [Tooltip("Eğer otomatik bulamazsa, oyuncuyu buraya elle sürükleyebilirsin.")]
        [SerializeField] private Transform _playerTransform;

        private void Start()
        {
            InitializeSystem();
        }

        private void InitializeSystem()
        {
            // 1. OYUNCUYU BUL (ETİKET İLE)
            // Eğer Inspector'dan elle atamadıysan, otomatik bulmayı dene.
            if (_playerTransform == null)
            {
                // "Player" etiketli objeyi bulur. (Script fark etmeksizin)
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                
                if (playerObj != null)
                {
                    _playerTransform = playerObj.transform;
                    Debug.Log("✅ Oyuncu bulundu: " + _playerTransform.name);
                }
                else
                {
                    Debug.LogError("❌ HATA: Sahnede 'Player' etiketli (Tag) bir obje yok! Lütfen karakterinin Tag'ini 'Player' yap.");
                    return; // Oyuncu yoksa kodun geri kalanı çalışmaz.
                }
            }

            // 2. DOLLY KONTROLÜ
            if (_dollyTransform == null)
            {
                Debug.LogError("❌ Hata: Dolly (Lokomotif) objesi atanmamış! Inspector'dan atamalısın.");
                // Hata almamak için geçici oluştur
                GameObject tempDolly = new GameObject("Temp_Dolly");
                if (_playerTransform != null) tempDolly.transform.position = _playerTransform.position;
                _dollyTransform = tempDolly.transform;
            }
            else
            {
                // Dolly'yi oyuncunun hizasına getir
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
            // Eğer oyuncu bulunamadıysa burası çalışmaz.
            if (_playerTransform == null || _dollyTransform == null) return;

            Vector3 playerPos = _playerTransform.position;
            Vector3 dollyPos = _dollyTransform.position;

            // B. SAĞ / SOL SINIRI (CLAMP)
            // Dolly'nin X konumuna göre sağa ve sola limit koyuyoruz.
            float minX = dollyPos.x - _xBoundLimit;
            float maxX = dollyPos.x + _xBoundLimit;
            
            playerPos.x = Mathf.Clamp(playerPos.x, minX, maxX);

            // C. ARKA SINIR (PUSH)
            // Oyuncu Dolly'den çok geride kalırsa (Kamera giderse), oyuncuyu ileri çek.
            float minZ = dollyPos.z - _maxLagDistance;
            
            if (playerPos.z < minZ)
            {
                playerPos.z = minZ; 
            }

            // Hesaplanan yeni pozisyonu oyuncuya uygula
            _playerTransform.position = playerPos;
        }
    }
}