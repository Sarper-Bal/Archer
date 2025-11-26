using ArcadeBridge.ArcadeIdleEngine.Interactables;
using ArcadeBridge.ArcadeIdleEngine.Items;
using ArcadeBridge.ArcadeIdleEngine.Storage;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Processors.Spawners
{
    [SelectionBase]
    public class Spawner : MonoBehaviour
    {
        [SerializeField] SpawnerDefinition _definition;
        [SerializeField] Inventory _inventory;
        [SerializeField] Timer _timer;
        
        void Update()
        {
            if (!_inventory.IsVisibleFull)
            {
                _timer.Tick();
                if (_timer.IsCompleted)
                {
                    Item item = _definition.SpawningItem.Pool.Get();
                    item.transform.SetPositionAndRotation(transform.position, Quaternion.identity);
                    _inventory.AddVisible(item);
                    _timer.SetZero();
                }
            }
        }
    }
}