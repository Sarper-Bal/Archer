using ArcadeBridge.ArcadeIdleEngine.Items;
using ArcadeBridge.ArcadeIdleEngine.Storage;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Interactables
{
	public class InventoryMonitor : MonoBehaviour
	{
		[SerializeField] Inventory _inventory;
		[SerializeField] MonitorEntrySpriteText[] _monitorEntry;
		[SerializeField] ItemDefinition[] _itemDefinitions;

		void Awake()
		{
			for (int i = 0; i < _monitorEntry.Length; i++)
			{
				MonitorEntrySpriteText monitorEntrySpriteText = _monitorEntry[i];
				monitorEntrySpriteText.Initialize(_itemDefinitions[i].Sprite, "0");
			}
		}

		void OnEnable()
		{
			_inventory.ItemAdded += Inventory_ItemAdded;
			_inventory.ItemRemoved += Inventory_ItemRemoved;
		}

		void OnDisable()
		{
			_inventory.ItemAdded -= Inventory_ItemAdded;
			_inventory.ItemRemoved -= Inventory_ItemRemoved;
		}

		void Inventory_ItemAdded(Item arg1, int arg2)
		{
			for (int i = 0; i < _itemDefinitions.Length; i++)
			{
				if (_itemDefinitions[i] == arg1.Definition)
				{
					_monitorEntry[i].SetText(arg2.ToString());
				}
			}
		}
		
		void Inventory_ItemRemoved(Item arg1, int arg2)
		{
			for (int i = 0; i < _itemDefinitions.Length; i++)
			{
				if (_itemDefinitions[i] == arg1.Definition)
				{
					_monitorEntry[i].SetText(arg2.ToString());
				}
			}
		}
	}
}
