using DG.Tweening;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Interactables
{
    public class InteractableTweener : MonoBehaviour
    {
        [SerializeField] Ease _tweenRotateEase;
        [SerializeField] Ease _tweenMoveEase;

        Tween _moveTween;
        Tween _rotateTween;

        void OnEnable()
        {
            if (_moveTween == null)
            {
                _rotateTween = transform.DORotate(new Vector3(0f, 360f, 0f), 2f).SetRelative().SetLoops(-1).SetAutoKill(false).SetEase(_tweenRotateEase);
                _moveTween = transform.DOMove(new Vector3(0f, 1f, 0f), 1f).SetRelative().SetLoops(-1, LoopType.Yoyo).SetAutoKill(false).SetEase(_tweenMoveEase);
            }
            else
            {
                _rotateTween.Play();
                _moveTween.Play();
            }
        }

        void OnDisable()
        {
            transform.DOPause();
        }
    }
}