using System;

namespace ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Core.ValidatorAttributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class MaxValueAttribute : ValidatorAttribute
    {
        public float MaxValue { get; private set; }

        public MaxValueAttribute(float maxValue)
        {
            MaxValue = maxValue;
        }

        public MaxValueAttribute(int maxValue)
        {
            MaxValue = maxValue;
        }
    }
}
