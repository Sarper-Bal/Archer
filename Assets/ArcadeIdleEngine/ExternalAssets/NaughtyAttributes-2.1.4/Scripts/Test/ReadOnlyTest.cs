using ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Core.DrawerAttributes;
using ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Core.MetaAttributes;
using UnityEngine;

namespace ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Test
{
    public class ReadOnlyTest : MonoBehaviour
    {
        [ReadOnly]
        public int readOnlyInt = 5;

        public ReadOnlyNest1 nest1;
    }

    [System.Serializable]
    public class ReadOnlyNest1
    {
        [ReadOnly]
        [AllowNesting]
        public float readOnlyFloat = 3.14f;

        public ReadOnlyNest2 nest2;
    }

    [System.Serializable]
    public struct ReadOnlyNest2
    {
        [ReadOnly]
        [AllowNesting]
        public string readOnlyString;
    }
}
