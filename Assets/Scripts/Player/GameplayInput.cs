using UnityEngine;

namespace Zazerkalye.Player
{
    /// <summary>Keyboard + on-screen stick share the same match input.</summary>
    public static class GameplayInput
    {
        public static Vector2 Stick;
        public static bool KokQueued;
        public static bool DashQueued;

        public static void QueueKok() => KokQueued = true;
        public static void QueueDash() => DashQueued = true;

        public static bool ConsumeKok()
        {
            if (!KokQueued) return false;
            KokQueued = false;
            return true;
        }

        public static bool ConsumeDash()
        {
            if (!DashQueued) return false;
            DashQueued = false;
            return true;
        }

        public static void Clear()
        {
            Stick = Vector2.zero;
            KokQueued = false;
            DashQueued = false;
        }
    }
}
