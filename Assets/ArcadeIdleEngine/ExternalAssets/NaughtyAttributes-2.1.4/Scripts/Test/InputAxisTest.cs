using ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Core.DrawerAttributes;
using ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Core.DrawerAttributes_SpecialCase;
using UnityEngine;

namespace ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Test
{
    public class InputAxisTest : MonoBehaviour
    {
        [InputAxis]
        public string inputAxis0;

        public InputAxisNest1 nest1;

        [Button]
        private void LogInputAxis0()
        {
            Debug.Log(inputAxis0);
        }
    }

    [System.Serializable]
    public class InputAxisNest1
    {
        [InputAxis]
        public string inputAxis1;

        public InputAxisNest2 nest2;
    }

    [System.Serializable]
    public struct InputAxisNest2
    {
        [InputAxis]
        public string inputAxis2;
    }
}
