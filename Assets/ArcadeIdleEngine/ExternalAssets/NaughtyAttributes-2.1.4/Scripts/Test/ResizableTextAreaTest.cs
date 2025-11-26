using ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Core.DrawerAttributes;
using UnityEngine;

namespace ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Test
{
    public class ResizableTextAreaTest : MonoBehaviour
    {
        [ResizableTextArea]
        public string text0;

        public ResizableTextAreaNest1 nest1;
    }

    [System.Serializable]
    public class ResizableTextAreaNest1
    {
        [ResizableTextArea]
        public string text1;

        public ResizableTextAreaNest2 nest2;
    }

    [System.Serializable]
    public class ResizableTextAreaNest2
    {
        [ResizableTextArea]
        public string text2;
    }
}
