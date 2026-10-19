using UnityEngine;
namespace TimeGate.World
{
    /// Written by the UI (UI Toolkit pointer events), read by spirit + camera.
    public static class TouchInput
    {
        public static Vector2 Move;        // x: strafe, y: forward/back
        public static float Vertical;      // -1 down .. +1 up
        public static Vector2 LookDelta;   // accumulated drag in panel units
        public static bool Boost;
        public static bool Slow;
        public static Vector2 ConsumeLook() { var l = LookDelta; LookDelta = Vector2.zero; return l; }
        public static void Reset() { Move = Vector2.zero; Vertical = 0; LookDelta = Vector2.zero; }
    }
}
