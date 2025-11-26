using System;
using ArcadeBridge.ArcadeIdleEngine.Interactables;
using ArcadeBridge.ArcadeIdleEngine.Items;
using DG.Tweening;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Helpers
{
    public static class TweenHelper
    {
        public static void JumpOrganized(Item item, Transform pivotPoint, RowColumnHeight rowColumnHeight, float jumpHeight, float jumpDuration, int index, TweenCallback onComplete)
        {
            Vector3 point = ArcadeIdleHelper.GetPoint(index, rowColumnHeight);
            Vector3 adjustedPos = pivotPoint.TransformPoint(point);
            Jump(item.transform, adjustedPos, jumpHeight, 1, jumpDuration, onComplete);
        }
        
        public static void JumpOrganized(Item item, Transform pivotPoint, RowColumnHeight rowColumnHeight, float jumpHeight, float jumpDuration, int index)
        {
            Vector3 point = ArcadeIdleHelper.GetPoint(index, rowColumnHeight);
            Vector3 adjustedPos = pivotPoint.TransformPoint(point);
            JumpAndRotate(item.transform, adjustedPos, Vector3.zero, jumpHeight, jumpDuration);
        }
        
        public static void JumpAndRotate(Transform item, Vector3 targetPosition, Vector3 targetRotation, float jumpPower, float duration)
        {
            item.DOJump(targetPosition, jumpPower, 1, duration).SetRecyclable().SetAutoKill();
            item.DOLocalRotate(targetRotation, duration).SetRecyclable().SetAutoKill();
        }
        
        public static void LocalJumpAndRotate(Transform item, Vector3 targetPosition, Vector3 targetRotation, float jumpPower, float duration)
        {
            item.DOLocalJump(targetPosition, jumpPower, 1, duration).SetRecyclable().SetAutoKill();
            item.DOLocalRotate(targetRotation, duration).SetRecyclable().SetAutoKill();
        }
        
        public static void LocalJumpAndRotate(Transform item, Vector3 targetPosition, Vector3 targetRotation, float jumpPower, float duration, TweenCallback onComplete)
        {
            item.DOLocalJump(targetPosition, jumpPower, 1, duration).SetRecyclable().SetAutoKill().OnComplete(onComplete);
            item.DOLocalRotate(targetRotation, duration).SetRecyclable().SetAutoKill();
        }

        public static void SpendResource(int requiredResource, int collectedResource, int myResource, out Tween resourceSpendingTween, float spendingDuration,
                                         Ease resourceSpendEase, TweenCallback<int> onTweenUpdate)
        {
            int remainedMoney = requiredResource - collectedResource;
            int to = myResource >= remainedMoney ? requiredResource : collectedResource + myResource;
            resourceSpendingTween = DOVirtual.Int(collectedResource, to, ((float)to / requiredResource) * spendingDuration, onTweenUpdate)
                .SetEase(resourceSpendEase).SetAutoKill();
        }

        public static void SetParentAndJump(this Transform transform, Transform to, Action onJumped)
        {
            transform.SetParent(to);
            transform.DOLocalJump(Vector3.zero, 1f, 1, 0.4f).SetRecyclable().SetAutoKill().OnComplete(() => onJumped?.Invoke());
            transform.DOScale(Vector3.kEpsilon, 0.4f).SetEase(Ease.InBack, 5f).SetRecyclable().SetAutoKill().OnComplete(() => transform.gameObject.SetActive(false));
        }

        public static Tweener LocalMove(Transform transform, Vector3 targetPoint, float duration)
        {
            return transform.DOLocalMove(targetPoint, duration).SetRecyclable();
        }
        
        public static Sequence Jump(Transform transform, Transform targetPoint)
        {
            return transform.DOJump(targetPoint.position, 1f, 1, 1f).SetRecyclable().SetAutoKill();
        }

        public static Sequence Jump(Transform transform, Vector3 targetPoint, float jumpPower, int numJumps, float duration)
        {
            return transform.DOJump(targetPoint, jumpPower, numJumps, duration).SetEase(Ease.Linear).SetRecyclable().SetAutoKill();
        }

        public static void Jump(Transform transform, Vector3 targetPoint, float jumpPower, int numJumps, float duration, TweenCallback onComplete)
        {
            transform.DOJump(targetPoint, jumpPower, numJumps, duration).SetRecyclable().SetAutoKill().OnComplete(onComplete);
        }

        public static void ShowSlowly(Transform transform, Vector3 targetScale, float duration, TweenCallback onComplete)
        {
            transform.gameObject.SetActive(true);
            transform.DOScale(targetScale, duration).SetEase(Ease.OutBack, 2f).OnComplete(onComplete);
        }

        public static void DisappearSlowlyAndDeactivate(Transform transform)
        {
            transform.DOScale(Vector3.one * Mathf.Epsilon, 0.2f).SetAutoKill().SetRecyclable().OnComplete(() => { transform.gameObject.SetActive(false); });
        }
        
        public static void ScaleDownSlowlyInBack(Transform transform, float duration)
        {
            transform.DOScale(Vector3.one * Mathf.Epsilon, duration).SetEase(Ease.InBack).SetAutoKill().SetRecyclable();
        }
        
        public static Tween DisappearSlowly(Transform transform, float duration, TweenCallback onCompleted)
        {
            return transform.DOScale(Vector3.one * Mathf.Epsilon, duration).SetEase(Ease.InBack, 2f).SetAutoKill().SetRecyclable().OnComplete(onCompleted);
        }

        public static void CompleteAll(Transform transform)
        {
            transform.DOComplete();
        }

        public static void KillAllTweens(Transform transform)
        {
            transform.DOKill();
        }

        public static void DelayedCall(float delay, TweenCallback callback)
        {
            DOVirtual.DelayedCall(delay, callback);
        }

        public static void ShakeScale(Transform trans, Vector3 targetScale, float duration)
        {
            trans.DOShakeScale(duration, targetScale).SetRecyclable();
        }
        
        public static void PunchScale(Transform trans, Vector3 targetScale, float duration)
        {
            trans.DOPunchScale(targetScale, duration, 2).SetRecyclable();
        }
    }
}