using ArcadeBridge.ArcadeIdleEngine.Data.Variables;
using TMPro;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Data.Monitors
{
	public class IntVariableMonitor : MonoBehaviour
	{
		[SerializeField] IntVariable _monitorVariable;
		[SerializeField] TextMeshProUGUI _monitorText;

		void OnEnable()
		{
			_monitorVariable.ValueSet += SetText;
		}
		
		void OnDisable()
		{
			_monitorVariable.ValueSet -= SetText;
		}

		void Start()
		{
			SetText(_monitorVariable.RuntimeValue);
		}

		void SetText(int obj)
		{
			_monitorText.text = obj.ToString();
		}
	}
}
