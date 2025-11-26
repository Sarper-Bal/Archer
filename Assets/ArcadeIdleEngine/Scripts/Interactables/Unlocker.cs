using System;
using System.Collections;
using ArcadeBridge.ArcadeIdleEngine.Helpers;
using ArcadeBridge.ArcadeIdleEngine.Items;
using ArcadeBridge.ArcadeIdleEngine.Storage;
using ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Core.MetaAttributes;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ArcadeBridge.ArcadeIdleEngine.Interactables
{
	public class Unlocker : MonoBehaviour
	{
		[Serializable]
		enum CompletionBehaviour
		{
			Destroy, Deactivate, Nothing
		}
		
		const int VISUAL_FEEDBACK_SPAWN_RATE_MAX = 40;

		[SerializeField, Tooltip("This will be called when enough resource spent.")] 
		UnityEvent _onUnlocked;
		
		[SerializeField, Tooltip("Text which shows how much resource do we have need to unlock."), BoxGroup("UI")] 
		TextMeshProUGUI _resourceCountText;
		
		[SerializeField, Tooltip("Image which shows how much resource do we have need to unlock."), BoxGroup("UI")] 
		Image _progressBar;
		
		[FormerlySerializedAs("_neededResource")]
		[SerializeField, Tooltip("Resource to spend when unlocking."), BoxGroup("Spending")] 
		ItemDefinition _requiredResource;

		[FormerlySerializedAs("_requiredResource")]
		[SerializeField, Tooltip("Amount of resource needed for unlocking."), Min(0), BoxGroup("Spending")] 
		int _requiredResourceAmount;
		
		[SerializeField, BoxGroup("Spending")] Ease _spendingSpeedCurve;

		[FormerlySerializedAs("_spendingSpeed")]
		[SerializeField, Range(0f, 10f), Tooltip("Time it takes to complete spending from 0 to requires resource amount"), BoxGroup("Spending")] 
		float _spendingDuration;
		
		[SerializeField, Tooltip("If true, then it will be locked again when it's unlocked so it can be unlocked multiple times."), BoxGroup("Spending")] 
		bool _workMultipleTimes;
		
		[SerializeField, Tooltip("controls how frequent resource object will be shown"), Range(1, VISUAL_FEEDBACK_SPAWN_RATE_MAX)] 
		int _visualFeedbackSpawnRate;
		
		[SerializeField, Range(0f, 10f)] float _jumpHeight;
		[SerializeField, Range(0f, 3f)] float _jumpDuration;
		[SerializeField, Tooltip("Visual feedback jump point. Feedback goes to this place and disappears.")] Transform _jumpPoint;
		
		[SerializeField, Tooltip("Triggered behaviour after Unlocker unlocks something. If workMultipleTimes is active, then this won't be called.")]
		CompletionBehaviour _completionBehaviour = CompletionBehaviour.Deactivate;

		Inventory _inventory;
		Tween _spendingTween;
		Coroutine _cor;
		WaitForSeconds _waitForSeconds;
		int _previousResourceSpentAmount;
		int _collectedResource;
		int _spawnCount;

		public event Action<Unlocker> Stopped;
		public event Action<Unlocker> Completed;

		void Awake() 
		{
			_waitForSeconds = new WaitForSeconds(0.1f);
		}
		
		void OnTriggerEnter(Collider other)
		{
			if (other.TryGetComponent(out Inventory inventory))
			{
				_inventory = inventory;
				_cor = StartCoroutine(CheckInventory(inventory));
			}
		}

		void OnTriggerExit(Collider other)
		{
			if (other.TryGetComponent(out Inventory _))
			{
				StopSpending();
				Stopped?.Invoke(this);
				_inventory = null;
			}
		}

		void OnValidate()
		{
			_resourceCountText.text = _requiredResourceAmount.ToString();
		}
		
		public void SetRequiredResource(int requiredResource)
		{
			_collectedResource = 0;
			_previousResourceSpentAmount = 0;
			_progressBar.fillAmount = 0;
			_requiredResourceAmount = requiredResource;
			_resourceCountText.text = _requiredResourceAmount.ToString();
		}

		IEnumerator CheckInventory(Inventory inventory)
		{
			while (true)
			{
				if (!inventory.Interactable)
				{
					yield return _waitForSeconds;
					continue;
				}

				int resourceAmount = _requiredResource.Variable.RuntimeValue;
				TweenHelper.SpendResource(_requiredResourceAmount, _collectedResource, resourceAmount, out _spendingTween, _spendingDuration, _spendingSpeedCurve, SpendMoney);
				yield break;
			}
		}

		void SpendMoney(int x)
		{
			int decreasingAmountDelta = x - _previousResourceSpentAmount;
			if (decreasingAmountDelta == 0)
			{
				return;
			}
			
			// First search the inventory, if we can't find, use variable and pools
			if (_inventory.TryRemove(_requiredResource, out Item item))
			{
				int amount = 0;
				TweenHelper.KillAllTweens(item.transform);
				TweenHelper.Jump(item.transform, _jumpPoint.position, _jumpHeight, 1, _jumpDuration, item.ReleaseToPool);
				amount++;
				while (amount < decreasingAmountDelta)
				{
					if (_inventory.TryRemove(_requiredResource, out Item it))
					{
						TweenHelper.KillAllTweens(it.transform);
						TweenHelper.Jump(it.transform, _jumpPoint.position, _jumpHeight, 1, _jumpDuration, it.ReleaseToPool);
						amount++;	
					}
				}
			}
			else
			{
				_requiredResource.Variable.RuntimeValue -= decreasingAmountDelta;
				_spawnCount++;
				if (_spawnCount >= VISUAL_FEEDBACK_SPAWN_RATE_MAX + 1 - _visualFeedbackSpawnRate)
				{
					Item poolInstance = _requiredResource.Pool.Get();
					Transform trans = poolInstance.transform;
					trans.position = _inventory.transform.position;
					TweenHelper.Jump(trans, _jumpPoint.position, _jumpHeight, 1, _jumpDuration, poolInstance.ReleaseToPool);
					_spawnCount = 0;
				}
			}
			_collectedResource += decreasingAmountDelta;
			_resourceCountText.text = (_requiredResourceAmount - _collectedResource).ToString();
			_previousResourceSpentAmount = x;
			_progressBar.fillAmount = (float)_collectedResource / _requiredResourceAmount;
			
			if (_collectedResource == _requiredResourceAmount)
			{
				_onUnlocked?.Invoke();
				Completed?.Invoke(this);
				StopSpending();

				if (_workMultipleTimes)
				{
					SetRequiredResource(_requiredResourceAmount);
					_cor = StartCoroutine(CheckInventory(_inventory));
				}
				else
				{
					if (_completionBehaviour == CompletionBehaviour.Destroy)
					{
						Destroy(gameObject);
					}
					else if (_completionBehaviour == CompletionBehaviour.Deactivate)
					{
						gameObject.SetActive(false);
					}
				}
			}
		}
		
		void StopSpending()
		{
			_spendingTween?.Kill();
			if (_cor != null)
			{
				StopCoroutine(_cor);				
			}
		}
	}
}