using System;
using ArcadeBridge.ArcadeIdleEngine.Interactables;
using ArcadeBridge.ArcadeIdleEngine.Items;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ArcadeBridge.ArcadeIdleEngine.Storage
{
	public class Inventory : MonoBehaviour
	{
		[SerializeField] InventoryInvisible _inventoryInvisible;
		[SerializeField] InventoryVisible _inventoryVisible;
		
		IInteractor _interactor;

		public event Action<Item, int> ItemAdded; 
		public event Action<Item, int> ItemRemoved; 

		public bool Interactable => _interactor.Interactable;
		public bool IsEmpty => _inventoryInvisible.IsEmpty() && _inventoryVisible.IsEmpty();
		public bool IsFull => _inventoryInvisible.IsFull() && _inventoryVisible.IsFull();
		public bool IsVisibleEmpty => _inventoryVisible.IsEmpty();
		public bool IsInvisibleEmpty => _inventoryInvisible.IsEmpty();
		public bool IsVisibleFull => _inventoryVisible.IsFull();
		public bool IsInvisibleFull => _inventoryInvisible.IsFull();

		void Awake()
		{
			_interactor = GetComponent<IInteractor>();
			if (_interactor == null)
			{
				_interactor = new AlwaysEnableInteraction();
			}
		}

		public bool Contains(ItemDefinition definition, out Item item)
		{
			return definition.Visible ? _inventoryVisible.Contains(definition, out item) : _inventoryInvisible.Contains(definition, out item);
		}

		public void SetVisibleCapacity(RowColumnHeight rowColumnHeight)
		{
			_inventoryVisible.SetCapacity(rowColumnHeight);
		}

		public void SetInvisibleCapacity(int capacity)
		{
			_inventoryInvisible.SetCapacity(capacity);
		}

		public void Add(Item item)
		{
			if (item.Definition.Visible)
			{
				AddVisible(item);
			}
			else
			{
				AddInvisible(item);
			}
		}

		public void AddVisible(Item item)
		{
			_inventoryVisible.Add(item);
			ItemAdded?.Invoke(item, _inventoryVisible.Count);
		}

		public void AddInvisible(Item item)
		{
			_inventoryInvisible.Add(item);
			ItemAdded?.Invoke(item, _inventoryInvisible.Count);
		}

        public bool CanAdd(ItemDefinition definition)
		{
			bool result = definition.Visible ? !_inventoryVisible.IsFull() : !_inventoryInvisible.IsFull();
            return result;
		}

        public void Remove(Item item)
		{
			if (item.Definition.Visible)
			{
				_inventoryVisible.Remove(item);
				ItemRemoved?.Invoke(item, _inventoryVisible.Count);
			}
			else
			{
				_inventoryInvisible.Remove(item);
				ItemRemoved?.Invoke(item, _inventoryInvisible.Count);
			}
		}
        
        public bool TryRemove(ItemDefinition definition, out Item item)
		{
			if (definition.Visible)
			{
				if (_inventoryVisible.Contains(definition, out item))
				{
					Remove(item);
					return true;
				}
			}
			else
			{
				if (_inventoryVisible.Contains(definition, out item))
				{
					Remove(item);
					return true;
				}	
			}

			return false;
		}

        public bool TryRemoveLastVisibleItem(out Item item)
        {
	        item = null;

	        if (_inventoryVisible.IsEmpty())
	        {
		        return false;
	        }

	        if (_inventoryVisible.TryRemoveLast(out item))
	        {
		        ItemRemoved?.Invoke(item, _inventoryVisible.Count);
	        }
	        return true;
        }
        
        public bool TryRemoveLastInvisibleItem(out Item item)
        {
	        item = null;

	        if (_inventoryInvisible.IsEmpty())
	        {
		        return false;
	        }
	        
	        if (_inventoryInvisible.TryRemoveLast(out item))
	        {
		        ItemRemoved?.Invoke(item, _inventoryInvisible.Count);
	        }
	        return true;
        }
        
        public bool TryRemoveRandomVisibleItem(out Item item)
        {
	        item = null;

	        if (_inventoryVisible.IsEmpty())
	        {
		        return false;
	        }

	        
	        _inventoryVisible.TryRemoveRandom(out item);
	        return true;
        }
        
        public bool TryRemoveRandomInvisibleItem(out Item item)
        {
	        item = null;

	        if (_inventoryInvisible.IsEmpty())
	        {
		        return false;
	        }

	        _inventoryInvisible.TryRemoveRandom(out item);
	        return true;
        }
        
        public bool TryRemoveRandomItem(out Item item)
		{
			if (_inventoryInvisible.IsEmpty() && _inventoryVisible.IsEmpty())
			{
				item = null;
				return false;
			}
			
			if (!_inventoryVisible.IsEmpty() && !_inventoryInvisible.IsEmpty())
			{
				if (Random.value > 0.5f)
				{
					return _inventoryInvisible.TryRemoveRandom(out item);
				}
			
				return _inventoryVisible.TryRemoveRandom(out item);
			}
			else if (!_inventoryVisible.IsEmpty())
			{
				return _inventoryVisible.TryRemoveRandom(out item);
			}
			else if (!_inventoryInvisible.IsEmpty())
			{
				return _inventoryInvisible.TryRemoveRandom(out item);
			}

			item = null;
			return false;
		}
	}
}
