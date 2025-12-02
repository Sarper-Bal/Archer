using UnityEngine;
using DG.Tweening; // DOTween gerekli

namespace ArcadeBridge.ArcadeIdleEngine.Tower
{
    // Seçilebilir animasyon türleri
    public enum TowerAnimMode
    {
        None,           // Kapalı
        PunchScale,     // Büyüyüp küçülme
        RecoilRotation, // Geri tepme (Namlu kalkar)
        Elastic         // Lastik gibi esneme
    }

    [RequireComponent(typeof(TowerAttacker))]
    public class TowerAnimationController : MonoBehaviour
    {
        [Header("⚙️ Genel Ayarlar")]
        [Tooltip("Hangi animasyonun oynayacağını seçin.")]
        [SerializeField] private TowerAnimMode _mode = TowerAnimMode.PunchScale;

        [Tooltip("Efektin uygulanacağı ana obje (Genelde 'Visuals' objesi).")]
        [SerializeField] private Transform _visualRoot;

        [Header("1. Punch Scale Ayarları")]
        [SerializeField] private Vector3 _punchVector = new Vector3(0.1f, 0.1f, 0.1f);
        [SerializeField] private float _punchDuration = 0.2f;
        [SerializeField] private int _vibrato = 10;

        [Header("2. Recoil Rotation Ayarları")]
        [Tooltip("Geri tepme açısı (Genelde X ekseninde eksi değer).")]
        [SerializeField] private Vector3 _recoilStrength = new Vector3(-15f, 0f, 0f);
        [SerializeField] private float _recoilDuration = 0.15f;

        [Header("3. Elastic Ayarları")]
        [SerializeField] private Vector3 _elasticStrength = new Vector3(0.1f, -0.1f, 0.1f);
        [SerializeField] private float _elasticDuration = 0.4f;

        private TowerAttacker _attacker;
        private Tween _cachedTween; // Memory Allocation (Çöp) oluşmaması için tween saklanır.

        private void Awake()
        {
            _attacker = GetComponent<TowerAttacker>();
        }

        private void Start()
        {
            InitializeTween();
        }

        /// <summary>
        /// Seçilen moda göre Tween'i bir kere oluşturur ve hazırlar.
        /// </summary>
        private void InitializeTween()
        {
            if (_visualRoot == null) return;

            // Varsa eski tween'i temizle
            if (_cachedTween != null) _cachedTween.Kill();

            switch (_mode)
            {
                case TowerAnimMode.PunchScale:
                    _cachedTween = _visualRoot.DOPunchScale(_punchVector, _punchDuration, _vibrato, 1)
                        .SetAutoKill(false).Pause();
                    break;

                case TowerAnimMode.RecoilRotation:
                    _cachedTween = _visualRoot.DOPunchRotation(_recoilStrength, _recoilDuration, 10, 1)
                        .SetAutoKill(false).Pause();
                    break;

                case TowerAnimMode.Elastic:
                    _cachedTween = _visualRoot.DOPunchScale(_elasticStrength, _elasticDuration, 4, 0.5f)
                        .SetEase(Ease.OutElastic)
                        .SetAutoKill(false).Pause();
                    break;
            }
        }

        private void OnEnable()
        {
            if (_attacker != null) _attacker.OnFired += PlayAnimation;
        }

        private void OnDisable()
        {
            if (_attacker != null) _attacker.OnFired -= PlayAnimation;
        }

        private void PlayAnimation()
        {
            if (_mode == TowerAnimMode.None || _cachedTween == null) return;

            // Tween'i başa sar ve oynat (En yüksek performans)
            _cachedTween.Restart();
        }

        // Editörde mod değiştirirseniz oyun içindeyken günceller
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                InitializeTween();
            }
        }
    }
}