using UnityEngine;
using UnityEngine.Events;

namespace ArcadeBridge.ArcadeIdleEngine.Interactables
{
	public class MultipleUnlocker : MonoBehaviour
	{
		[SerializeField] int _unlockerCount = 2;
		[SerializeField] UnityEvent _onUnlocked;
		
		int _currentUnlocked;
		
		public void Unlock()
		{
			_currentUnlocked++;
			if (_currentUnlocked >= _unlockerCount)
			{
				_onUnlocked.Invoke();
			}
		}
	}
}
