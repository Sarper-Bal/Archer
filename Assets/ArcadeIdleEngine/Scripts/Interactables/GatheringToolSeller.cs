using ArcadeBridge.ArcadeIdleEngine.Data;
using ArcadeBridge.ArcadeIdleEngine.Data.Variables;
using ArcadeBridge.ArcadeIdleEngine.Economy;
using ArcadeBridge.ArcadeIdleEngine.Gathering;
using UnityEngine;
using UnityEngine.Events;

namespace ArcadeBridge.ArcadeIdleEngine.Interactables
{
	public class GatheringToolSeller : MonoBehaviour
	{
		[SerializeField] IntVariable _money;
		[SerializeField] FloatingTextResourceAnimator _floatingTextResourceAnimator;
		[SerializeField] UniqueIntListVariable _toolList;
		[SerializeField] GatheringToolDefinition _gatheringToolDefinition;
		[SerializeField] int _price;
		[SerializeField] UnityEvent _buyCompleted;

		void Start()
		{
			if (_toolList.Contains(_gatheringToolDefinition.DatabaseIndex))
			{
				_buyCompleted.Invoke();
			}
		}

		public void Buy()
		{
			if (_money.RuntimeValue >= _price)
			{
				_floatingTextResourceAnimator.Play(transform, Camera.main.transform, _price);
				_money.RuntimeValue -= _price;
				_toolList.AddElement(_gatheringToolDefinition.DatabaseIndex);
				_buyCompleted.Invoke();
			}
		}
	}
}
