using System;
using System.Collections.Generic;
using ArcadeBridge.ArcadeIdleEngine.Items;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ArcadeBridge.ArcadeIdleEngine.Storage
{
    [Serializable]
    public abstract class InventoryBase
    {
        [SerializeField] protected Transform StackingPoint;
        [SerializeField] protected float PickUpDuration = 0.3f;

        protected List<Item> Items = new List<Item>();

        public int Count => Items.Count;

        public bool IsEmpty()
        {
            return Items.Count == 0;
        }

        public bool TryRemoveRandom(out Item item)
        {
            if (Items.Count > 0)
            {
                int rnd = Random.Range(0, Items.Count);
                item = Items[rnd];
                Remove(item);
                return true;
            }

            item = null;
            return false;
        }
        
        public bool TryRemoveLast(out Item item)
        {
            if (Items.Count > 0)
            {
                item = Items[Items.Count - 1];
                Remove(item);
                return true;
            }

            item = null;
            return false;
        }

        public bool Contains(ItemDefinition definition, out Item result)
        {
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                Item it = Items[i];
                if (it.Definition == definition)
                {
                    result = it;
                    return true;
                }
            }

            result = null;
            return false;
        }

        public void Add(Item p)
        {
            OnAdding(p);
            Items.Add(p);
        }
        
        public void Remove(Item item)
        {
            Items.Remove(item);
            item.transform.SetParent(null);
            item.gameObject.SetActive(true);
            OnRemoved(item);
        }

        public abstract bool IsFull();

        protected abstract void OnAdding(Item item);
        protected virtual void OnRemoved(Item item)
        {
        }
    }
}