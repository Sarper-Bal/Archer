using ArcadeBridge.ArcadeIdleEngine.Items;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Pools
{
    [CreateAssetMenu(menuName = nameof(ArcadeIdleEngine) + "/" + nameof(Pools) + "/" + nameof(ItemPool))]
    public class ItemPool : ObjectPool<Item>
    {
        public ItemDefinition ItemDefinition => Behaviour.Definition;
    }
}