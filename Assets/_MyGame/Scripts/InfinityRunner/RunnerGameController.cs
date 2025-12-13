using UnityEngine;
using ArcadeBridge.ArcadeIdleEngine.Actors; 

namespace IndianOceanAssets.Engine2_5D.Managers
{
    public class RunnerGameController : MonoBehaviour
    {
        [Header("🚄 Koşu ve Hız Ayarları")]
        [Tooltip("Oyunun ileri akış hızı (Z ekseni).")]
        [SerializeField] private float _scrollSpeed = 10.0f;

        [Tooltip("Karakterin SAĞA/SOLA gitme hızı.")]
        [SerializeField] private float _sideMovementSensitivity = 15.0f;

        [Tooltip("Karakterin İLERİ/GERİ gitme hızı.")]
        [SerializeField] private float _forwardMovementSensitivity = 10.0f;

        [Header("🛡️ Sınır Ayarları (Kafes)")]
        [SerializeField] private float _xBoundLimit = 4.5f;
        [SerializeField] private float _maxLagDistance = 8.0f;
        [SerializeField] private bool _limitForwardMovement = true;
        [SerializeField] private float _maxForwardDistance = 6.0f;

        [Header("🎥 ve 🎮 Bağlantılar")]
        [SerializeField] private Transform _dollyTransform;
        [SerializeField] private Transform _playerTransform;
        
        [Tooltip("Kullanılan UI Joystick Kanalı.")]
        [SerializeField] private InputChannel _inputChannel; 

        [Header("🎭 Animasyon Ayarları")]
        [Tooltip("Animator'daki koşma parametresinin tam adı (Örn: Speed, Velocity, Move).")]
        [SerializeField] private string _animationParamName = "Speed"; 

        // --- Özel Değişkenler ---
        private Animator _playerAnimator;
        private int _animParamID;
        private ArcadeIdleMover _originalMover;
        private Vector2 _currentJoystickInput; 
        private bool _animatorHasParam = false;

        private void Start()
        {
            InitializeSystem();
        }

        private void OnEnable()
        {
            if (_inputChannel != null)
            {
                _inputChannel.JoystickUpdate += OnJoystickUpdated;
                _inputChannel.PointerUp += OnPointerUp;
            }
        }

        private void OnDisable()
        {
            if (_inputChannel != null)
            {
                _inputChannel.JoystickUpdate -= OnJoystickUpdated;
                _inputChannel.PointerUp -= OnPointerUp;
            }
            if (_originalMover != null) _originalMover.enabled = true;
        }

        private void OnJoystickUpdated(Vector2 value)
        {
            _currentJoystickInput = value;
        }

        private void OnPointerUp()
        {
            _currentJoystickInput = Vector2.zero;
        }

        private void InitializeSystem()
        {
            // 1. OYUNCUYU BUL
            if (_playerTransform == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null) _playerTransform = playerObj.transform;
                else
                {
                    Debug.LogError("❌ HATA: 'Player' etiketli obje bulunamadı!");
                    return;
                }
            }

            // 2. ÇAKIŞAN SCRIPTI KAPAT
            _originalMover = _playerTransform.GetComponent<ArcadeIdleMover>();
            if (_originalMover != null)
            {
                _originalMover.enabled = false; 
            }

            // 3. ANIMATOR KONTROLÜ (HATA VERMEMESİ İÇİN)
            _playerAnimator = _playerTransform.GetComponentInChildren<Animator>();
            if (_playerAnimator != null)
            {
                // Parametre var mı diye kontrol etmiyoruz ama ID'sini alıyoruz.
                // Eğer yanlış isim girildiyse LogWarning ile uyaracağız.
                _animParamID = Animator.StringToHash(_animationParamName);
                
                // Basit bir kontrol yapısı (Reflection ile parametre var mı bakmak pahalıdır, 
                // bu yüzden hata yakalamayı Runtime'da yapmayıp kullanıcıya bırakıyoruz).
                // Koşma animasyonunu varsayılan olarak 1.0f yap.
                _playerAnimator.SetFloat(_animParamID, 1.0f);
                _animatorHasParam = true; 
            }

            // 4. DOLLY KONTROLÜ
            if (_dollyTransform == null)
            {
                Debug.LogError("❌ Hata: Runner_Dolly atanmamış!");
                // Acil durum: Dolly yoksa mecburen oyuncunun olduğu yerde yarat
                GameObject tempDolly = new GameObject("Temp_Dolly");
                tempDolly.transform.position = _playerTransform.position;
                _dollyTransform = tempDolly.transform;
            }
            
            // [DÜZELTME] Dolly'yi oyuncunun üstüne ışınlayan kodu sildik!
            // Artık sen sahneye (Scene) nasıl yerleştirdiysen öyle başlar.
        }

        private void Update()
        {
            if (_dollyTransform == null || _playerTransform == null) return;

            float dt = Time.deltaTime;

            // A. KAMERAYI (DOLLY) SÜREKLİ İLERLET
            _dollyTransform.Translate(Vector3.forward * _scrollSpeed * dt);

            // B. KARAKTER HAREKETİ
            HandlePlayerMovement(dt);
        }

        private void HandlePlayerMovement(float dt)
        {
            float inputX = 0f;
            float inputZ = 0f; // [YENİ] İleri/Geri girdisi

            // 1. Kanal üzerinden gelen veriyi kullan
            if (_inputChannel != null)
            {
                inputX = _currentJoystickInput.x;
                inputZ = _currentJoystickInput.y; // [YENİ] Vertical veriyi alıyoruz
            }
            // 2. Klavye/Mouse (Yedek)
            else
            {
                inputX = Input.GetAxis("Horizontal");
                inputZ = Input.GetAxis("Vertical");
                
                if (Input.GetMouseButton(0))
                {
                    float mouseX = (Input.mousePosition.x / Screen.width) - 0.5f;
                    inputX = Mathf.Clamp(mouseX * 2f, -1f, 1f);
                }
            }

            Vector3 currentPos = _playerTransform.position;

            // --- SAĞ / SOL HAREKETİ ---
            float moveX = inputX * _sideMovementSensitivity * dt;
            currentPos.x += moveX;

            // --- İLERİ / GERİ HAREKETİ [YENİ] ---
            // Temel Hız (_scrollSpeed) + Oyuncu Girdisi (inputZ * sensitivity)
            // Eğer oyuncu hiçbir şeye basmazsa Dolly ile aynı hızda (_scrollSpeed) gider.
            // İleri basarsa hızlanır, geri basarsa yavaşlar.
            float moveZ = (_scrollSpeed + (inputZ * _forwardMovementSensitivity)) * dt;
            currentPos.z += moveZ; 

            // Pozisyonu güncelle
            _playerTransform.position = currentPos;
            _playerTransform.rotation = Quaternion.identity; 
        }

        private void LateUpdate()
        {
            if (_playerTransform == null || _dollyTransform == null) return;

            Vector3 playerPos = _playerTransform.position;
            Vector3 dollyPos = _dollyTransform.position;

            // C. SINIRLAR (KAFES SİSTEMİ)

            // 1. Yan Sınırlar
            playerPos.x = Mathf.Clamp(playerPos.x, -_xBoundLimit, _xBoundLimit);

            // 2. Dikey Sınırlar (Dolly'ye göre hesaplanır)
            float minZ = dollyPos.z - _maxLagDistance;
            
            // Eğer ileri sınır kapalıysa sonsuza gidebilir, açıksa sınırla
            float maxZ = _limitForwardMovement ? (dollyPos.z + _maxForwardDistance) : Mathf.Infinity;
            
            playerPos.z = Mathf.Clamp(playerPos.z, minZ, maxZ);

            _playerTransform.position = playerPos;
        }
    }
}