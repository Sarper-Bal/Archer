using System.Diagnostics;

namespace ArcadeBridge.ArcadeIdleEngine.Experimental
{
    public static class EditorOnlyDebugger
    {
        [Conditional("ENABLE_LOGS")]
        public static void Log(string logMsg)
        {
            UnityEngine.Debug.Log(logMsg);
        }
    }
}