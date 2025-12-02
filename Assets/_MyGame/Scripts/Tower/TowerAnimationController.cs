using UnityEngine;
using DG.Tweening; // DOTween kütüphanesini kullanıyoruz

namespace ArcadeBridge.ArcadeIdleEngine.Tower
{
    [RequireComponent(typeof(TowerAttacker))]
    public class TowerAnimationController : MonoBehaviour
    {
        [Header("💥 Animasyon Ayarları")]
        [Tooltip("Ateş ederken esneyecek/sallanacak ana görsel obje (Visual Root).")]
        [SerializeField] private Transform _visualRoot;

        [Tooltip("Geri tepme/Sallanma gücü.")]
        [SerializeField] private float _punchStrength = 0.2f;

        [Tooltip("Efektin süresi.")]
        [SerializeField] private float _duration = 0.15f;

        private TowerAttacker _attacker;
        private Tween _recoilTween; // Animasyonu önbelleğe (cache) alıyoruz

        private void Awake()
        {
            _attacker = GetComponent<TowerAttacker>();
        }

        private void Start()
        {
            // Animasyonu oyun başında bir kere oluşturup durduruyoruz (Memory Optimization)
            if (_visualRoot != null)
            {
                _recoilTween = _visualRoot.DOPunchScale(Vector3.one * _punchStrength, _duration, 2, 1)
                    .SetAutoKill(false) // Otomatik yok etme, tekrar kullanacağız
                    .Pause(); // Başta durdur
            }
        }

        private void OnEnable()
        {
            // Event'e abone ol: Ateş edilirse PlayRecoil çalışsın
            if (_attacker != null)
                _attacker.OnFired += PlayRecoil;
        }

        private void OnDisable()
        {
            // Abonelikten çık (Memory Leak önlemek için önemli)
            if (_attacker != null)
                _attacker.OnFired -= PlayRecoil;
        }

        private void PlayRecoil()
        {
            if (_recoilTween != null)
            {
                // Tween'i başa sar ve oynat (Yeni instance yaratmadan)
                _recoilTween.Restart(); 
            }
        }
    }
}