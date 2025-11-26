using System.Collections;
using System.Collections.Generic;
using ArcadeBridge.ArcadeIdleEngine.Items;
using ArcadeBridge.ArcadeIdleEngine.Storage;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Interactables
{
    /// <summary>
    /// Gives Item to the Inventory when the Inventory enters the trigger area. Multiple InventoryManager can't collect simultaneously.
    /// </summary>
    public class InventoryFeederTriggerArea : MonoBehaviour
    {
        [SerializeField] Inventory _inventory;
        [SerializeField] Timer _timer;

        Dictionary<Inventory, Coroutine> _coroutineDictionary = new Dictionary<Inventory, Coroutine>();

        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out Inventory inventoryManager))
            {
                _coroutineDictionary.Add(inventoryManager, StartCoroutine(Co_Feed(inventoryManager)));
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out Inventory inventoryManager))
            {
                StopCoroutine(_coroutineDictionary[inventoryManager]);
                _coroutineDictionary.Remove(inventoryManager);
            }
        }

        IEnumerator Co_Feed(Inventory inventory)
        {
            while (true)
            {
                if (!inventory.Interactable || _inventory.IsEmpty)
                {
                    yield return null;
                    continue;
                }

                if (_timer.IsCompleted)
                {
                    if (!inventory.IsVisibleFull && _inventory.TryRemoveLastVisibleItem(out Item visibleItem))
                    {
                        inventory.Add(visibleItem);
                        _timer.SetZero();
                    }
                    else if (!inventory.IsInvisibleFull && _inventory.TryRemoveLastInvisibleItem(out Item invisibleItem))
                    {
                        inventory.Add(invisibleItem);
                        _timer.SetZero();
                    }
                }
                else
                {
                    _timer.Tick();
                }
                
                yield return null;
            }
        }
    }
}