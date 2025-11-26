using ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Core.DrawerAttributes;
using UnityEngine;

namespace ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Test
{
    public class SceneTest : MonoBehaviour
    {
        [Scene]
        public string scene0;

        public SceneNest1 nest1;
    }

    [System.Serializable]
    public class SceneNest1
    {
        [Scene]
        public string scene1;

        public SceneNest2 nest2;
    }

    [System.Serializable]
    public struct SceneNest2
    {
        [Scene]
        public int scene2;
    }
}
