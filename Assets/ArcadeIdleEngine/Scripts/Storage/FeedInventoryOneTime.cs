using ArcadeBridge.ArcadeIdleEngine.Items;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Storage
{
	public class FeedInventoryOneTime : MonoBehaviour
	{
		[SerializeField] Inventory _inventory;
		[SerializeField] Item _item;

		void Start()
		{
			_inventory.Add(_item);
		}
	}
}
