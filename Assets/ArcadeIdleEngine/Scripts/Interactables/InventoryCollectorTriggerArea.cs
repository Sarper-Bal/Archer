using System.Collections;
using System.Collections.Generic;
using ArcadeBridge.ArcadeIdleEngine.Items;
using ArcadeBridge.ArcadeIdleEngine.Storage;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Interactables
{
	[SelectionBase]
	public class InventoryCollectorTriggerArea : MonoBehaviour
	{
		[SerializeField] ItemDefinitionCountPair[] _itemsToCollect;
		[SerializeField] Timer _collectingIntervalTimer;
		[SerializeField] Inventory _outputInventory;

		Dictionary<Inventory, Coroutine> _coroutineDictionary = new Dictionary<Inventory, Coroutine>();
		ItemDefinitionCountPair[] _currentItemsToCollect;

		void Awake()
		{
			_currentItemsToCollect = new ItemDefinitionCountPair[_itemsToCollect.Length];
			for (int i = 0; i < _currentItemsToCollect.Length; i++)
			{
				_currentItemsToCollect[i].ItemDefinition = _itemsToCollect[i].ItemDefinition;
				_currentItemsToCollect[i].Count = _itemsToCollect[i].Count;
			}
		}

		void OnTriggerEnter(Collider other)
		{
			if (other.TryGetComponent(out Inventory inventory))
			{
				_coroutineDictionary.Add(inventory, StartCoroutine(Co_Collect(inventory)));
			}
		}
		
		void OnTriggerExit(Collider other)
		{
			if (other.TryGetComponent(out Inventory inventory))
			{
				StopCoroutine(_coroutineDictionary[inventory]);
				_coroutineDictionary.Remove(inventory);
				_collectingIntervalTimer.SetZero();
			}
		}

		IEnumerator Co_Collect(Inventory inventory)
		{
			while (true)
			{
				if (_currentItemsToCollect.Length == 0 || !inventory.Interactable || _outputInventory.IsVisibleFull)
				{
					yield return null;
					continue;
				}

				if (_collectingIntervalTimer.IsCompleted)
				{
					for (int i = 0; i < _currentItemsToCollect.Length; i++)
					{
						if ((_itemsToCollect[i].Count > 0 && _currentItemsToCollect[i].Count <= 0) || !inventory.Contains(_currentItemsToCollect[i].ItemDefinition, out Item item))
						{
							continue;
						}

						_currentItemsToCollect[i].Count--;
						inventory.Remove(item);
						_outputInventory.Add(item);
						_collectingIntervalTimer.SetZero();

						bool shouldRefresh = true;
						foreach (ItemDefinitionCountPair currentItem in _currentItemsToCollect)
						{
							if (currentItem.Count > 0)
							{
								shouldRefresh = false;
							}
						}

						if (shouldRefresh)
						{
							for (int index = 0; index < _currentItemsToCollect.Length; index++)
							{
								_currentItemsToCollect[index].Count = _itemsToCollect[index].Count;
							}
						}
						break;
					}
				}
				else
				{
					_collectingIntervalTimer.Tick();
				}

				yield return null;
			}
		}
	}
}
