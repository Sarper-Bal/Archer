using ArcadeBridge.ArcadeIdleEngine.Economy;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Pools
{
    [CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(Pools) + "/" + nameof(AnimatedResourceEntityPool))]
    public class AnimatedResourceEntityPool : ObjectPool<AnimatedResourceEntity>
    {
    }
}
