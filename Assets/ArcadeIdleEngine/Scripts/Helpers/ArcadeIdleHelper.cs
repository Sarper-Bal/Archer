using ArcadeBridge.ArcadeIdleEngine.Interactables;
using UnityEngine;

namespace ArcadeBridge.ArcadeIdleEngine.Helpers
{
	public static class ArcadeIdleHelper
	{
		/// <summary>
		/// Feed it with index and layout and it returns point in the layout. 
		/// </summary>
		/// <param name="currentIndex"></param>
		/// <param name="rowColumnHeight"></param>
		/// <returns></returns>
		public static Vector3 GetPoint(int currentIndex, RowColumnHeight rowColumnHeight)
		{
			float maxRowWidth = (rowColumnHeight.RowCount - 1) * rowColumnHeight.RowOffset;
			float maxColumnWidth = (rowColumnHeight.ColumnCount - 1) * rowColumnHeight.ColumnOffset;
			int columnIndex = currentIndex % rowColumnHeight.ColumnCount;
			int rowIndex = currentIndex / rowColumnHeight.ColumnCount % rowColumnHeight.RowCount;
			int heightIndex = currentIndex / (rowColumnHeight.RowCount * rowColumnHeight.ColumnCount);
			Vector3 up = Vector3.up * (rowColumnHeight.HeightOffset * heightIndex);
			Vector3 right = Vector3.right * (columnIndex * rowColumnHeight.ColumnOffset - maxColumnWidth / 2f);
			Vector3 forward = Vector3.forward * (rowIndex * rowColumnHeight.RowOffset - maxRowWidth / 2f);
			Vector3 targetPos = up + right + forward;
			return targetPos;
		}
	}
}
