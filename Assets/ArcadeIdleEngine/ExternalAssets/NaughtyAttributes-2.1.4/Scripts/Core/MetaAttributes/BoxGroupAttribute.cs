using System;

namespace ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Core.MetaAttributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class BoxGroupAttribute : MetaAttribute, IGroupAttribute
    {
        public string Name { get; private set; }

        public BoxGroupAttribute(string name = "")
        {
            Name = name;
        }
    }
}
