using System;
using ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Core.DrawerAttributes;
using ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Core.DrawerAttributes_SpecialCase;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Data.Variables
{
    public abstract class Saveable<T> : Saveable
    {
        [SerializeField] T _initialValue;
        [NonSerialized, ShowNonSerializedField] T _runtimeValue;

#if UNITY_EDITOR
        [SerializeField, InfoBox("Specify a value and override Runtime Value. Useful for debugging.")] T _overrideValue;
#endif
        public event Action<T> ValueSet;
        public override object GetDefaultValue => _initialValue;
        
        public T RuntimeValue
        {
            get => _runtimeValue;
            set
            {
                _runtimeValue = value;
                ValueSet?.Invoke(_runtimeValue);
            }
        }

        protected abstract void OnEnable();

        public override object CaptureState()
        {
            return RuntimeValue;
        }

        protected void OnValueChanged(T t)
        {
            ValueSet?.Invoke(t);
        }

#if UNITY_EDITOR
        [Button]
        void OverrideRuntimeValue()
        {
            RuntimeValue = _overrideValue;
        }
#endif
    }

    public abstract class Saveable : ScriptableObject
    {
        [SerializeField, InfoBox("This is the key that will be used in the save file. Don't change this after you released your game unless you know what you are doing!")] 
        string _saveId;

        public string SaveId => _saveId;

        public abstract object GetDefaultValue { get; }

        public abstract void RestoreState(object obj);

        public abstract object CaptureState();

    }
}