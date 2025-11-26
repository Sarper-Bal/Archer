using System;
using ArcadeBridge.ArcadeIdleEngine.Gathering;
using ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Core.DrawerAttributes;
using ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Core.MetaAttributes;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Actors
{
    public class HumanoidAnimationManager : MonoBehaviour
    {
        [SerializeField] Animator _animator;
        [SerializeField, AnimatorParam(nameof(_animator))] string _interactionName;
        [SerializeField, AnimatorParam(nameof(_animator))] string _interactionSpeedName;
        [SerializeField, AnimatorParam(nameof(_animator))] string _moveName;
        
        [SerializeField, Tooltip("Allows you to adjust the movement parameter that will be fed into the animator. Normally, if our character moves 1m/s speed,"
             + " it will feed 1 to the Animator as a Move parameter.")] 
        float _movementMultiplier = 0.1f;
        
        [SerializeField] bool _useGatherer;
        [SerializeField, ShowIf(nameof(_useGatherer))] Gatherer _gatherer;

        int _moveId;
        int _interactionId;
        int _interactionSpeedId;
        float _velocity;
        float _currentVelocity;
        Vector3 _lastPosition;

        void Awake()
        {
            _moveId = Animator.StringToHash(_moveName);
            _interactionId = Animator.StringToHash(_interactionName);
            _interactionSpeedId = Animator.StringToHash(_interactionSpeedName);
            _lastPosition = transform.position;
        }

        void OnEnable()
        {
            if (_useGatherer)
            {
                _gatherer.Starting += Gatherer_Starting;
                _gatherer.Stopping += Gatherer_Stopping;
            }
        }

        void OnDisable()
        {
            if (_useGatherer)
            {
                _gatherer.Starting -= Gatherer_Starting;
                _gatherer.Stopping -= Gatherer_Stopping;
            }
        }

        void LateUpdate()
        {
            Vector3 position = transform.position;
            Vector3 deltaPosition = _lastPosition - position;
            float target = deltaPosition.magnitude * (1f / Time.deltaTime) * _movementMultiplier;
            _velocity = Mathf.SmoothDamp(_velocity, target, ref _currentVelocity, 0.05f);
            _animator.SetFloat(_moveId, _velocity);
            _lastPosition = position;
        }

        void Gatherer_Starting(GatheringTool gatheringTool)
        {
            _animator.SetFloat(_interactionSpeedId, 1f / gatheringTool.GatheringToolDefinition.UseInterval);
            _animator.SetInteger(_interactionId, gatheringTool.GatheringToolDefinition.InteractionAnimationId);
        }
        
        void Gatherer_Stopping()
        {
            _animator.SetInteger(_interactionId, -1);
        }
    }
}