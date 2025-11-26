using System;

namespace ArcadeIdleEngine.ExternalAssets.NaughtyAttributes_2._1._4.Core.DrawerAttributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ShowAssetPreviewAttribute : DrawerAttribute
    {
        public const int DefaultWidth = 64;
        public const int DefaultHeight = 64;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public ShowAssetPreviewAttribute(int width = DefaultWidth, int height = DefaultHeight)
        {
            Width = width;
            Height = height;
        }
    }
}
