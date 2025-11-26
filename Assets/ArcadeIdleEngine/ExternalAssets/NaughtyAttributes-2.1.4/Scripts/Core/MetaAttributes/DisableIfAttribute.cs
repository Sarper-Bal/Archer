using System;
using ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Core.Utility;

namespace ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Core.MetaAttributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class DisableIfAttribute : EnableIfAttributeBase
    {
        public DisableIfAttribute(string condition)
            : base(condition)
        {
            Inverted = true;
        }

        public DisableIfAttribute(EConditionOperator conditionOperator, params string[] conditions)
            : base(conditionOperator, conditions)
        {
            Inverted = true;
        }

        public DisableIfAttribute(string enumName, object enumValue)
            : base(enumName, enumValue as Enum)
        {
            Inverted = true;
        }
    }
}
