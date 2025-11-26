using ArcadeBridge.ArcadeIdleEngine.Helpers;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Items
{
    public class Item : MonoBehaviour
    {
        [SerializeField] ItemDefinition _definition;
        
        Vector3 _defaultLocalScale;
        
        public ItemDefinition Definition => _definition;

        void Awake()
        {
            _defaultLocalScale = transform.localScale;
        }

        public void ReleaseToPool()
        {
            transform.localScale = _defaultLocalScale;
            TweenHelper.KillAllTweens(transform);
            _definition.Pool.Release(this);
        }
    }
}