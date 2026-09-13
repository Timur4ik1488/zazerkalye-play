namespace Zazerkalye.Data
{
    public static class MatchConfig
    {
        public const float MatchSeconds = 150f;
        public const float WorldRadius = 42f;
        public const float KokRange = 2.4f;
        public const float KokCooldown = 0.28f;
        public const float DashCooldown = 1.15f;
        public const float DashDuration = 0.22f;
        public const float DashSpeed = 18f;
        public const float MoveSpeed = 6.2f;
        public const float ComboWindow = 2.6f;
        public const int MaxHp = 3;
        public const float HurtInvuln = 1.1f;
        public const float SpawnGrace = 2.5f;
        public const float NightAtSecondsLeft = 60f;
    }

    public enum MobKind { Bobyl, Hard, Jvachnik }
    public enum PowerKind { Magnet, Rage, Haste, Shield, Feast }

    public struct MatchResult
    {
        public int Kukichi;
        public int Kokked;
        public int Shards;
        public bool Perfect;
        public bool MirrorFound;
        public bool EyeFound;
        public int Score;
        public int MaxCombo;
        public int DamageTaken;
    }
}
