using System;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Interactables
{
    [Serializable]
    public struct RowColumnHeight
    {
        [Header("Counts")]
        public int RowCount;
        public int ColumnCount;
        public int HeightCount;
        
        [Header("Offsets")]
        public float RowOffset;
        public float ColumnOffset;
        public float HeightOffset;

        public int GetCapacity()
        {
            return RowCount * ColumnCount * HeightCount;
        }
    }
}