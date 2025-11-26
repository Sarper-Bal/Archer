using ArcadeBridge.ArcadeIdleEngine.Items;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Storage
{
	public class InventoryRegisterer : MonoBehaviour
	{
		[SerializeField] Inventory _inventory;

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
		
		void Inventory_ItemRemoved(Item p, int arg2)
		{
			if (p.Definition.Variable)
			{
				p.Definition.Variable.RuntimeValue -= 1;
			}	
		}
		
		void Inventory_ItemAdded(Item p, int arg2)
		{
			if (p.Definition.Variable)
			{
				p.Definition.Variable.RuntimeValue += 1;
			}
		}
	}
}
