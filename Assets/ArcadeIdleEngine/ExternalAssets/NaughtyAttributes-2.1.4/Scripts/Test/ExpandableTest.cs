using ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Core.DrawerAttributes;
using UnityEngine;

namespace ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Test
{
    public class ExpandableTest : MonoBehaviour
    {
        // See #294
        public int precedingField = 5;

        [Expandable]
        public ScriptableObject obj0;

        public ExpandableScriptableObjectNest1 nest1;
    }

    [System.Serializable]
    public class ExpandableScriptableObjectNest1
    {
        [Expandable]
        public ScriptableObject obj1;

        public ExpandableScriptableObjectNest2 nest2;
    }

    [System.Serializable]
    public class ExpandableScriptableObjectNest2
    {
        [Expandable]
        public ScriptableObject obj2;
    }
}
