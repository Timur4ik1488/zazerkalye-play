using System.Collections.Generic;
using UnityEngine;
using Zazerkalye.Data;
using Zazerkalye.Enemies;
using Zazerkalye.Player;
using Zazerkalye.World;
using Zazerkalye.Visual;

namespace Zazerkalye.Core
{
    public class MatchController : MonoBehaviour
    {
        public PlayerController Player;
        public ThirdPersonCamera Cam;
        public Transform WorldRoot;
        public System.Action<MatchResult> OnFinished;
        public System.Action<string> OnToast;
        public System.Action OnHudDirty;

        readonly List<Enemy> _enemies = new();
        readonly List<Pickup> _pickups = new();

        float _timeLeft = MatchConfig.MatchSeconds;
        int _kukichi, _kokked, _shards, _score, _combo, _maxCombo;
        float _comboTimer;
        int _hp = MatchConfig.MaxHp;
        int _damageTaken;
        bool _mirrorFound, _eyeFound, _ended, _night;
        int _wave, _swampDepth;
        bool _wave1, _wave2, _wave4, _mirrorHint;

        public float TimeLeft => _timeLeft;
        public int Kukichi => _kukichi;
        public int Shards => _shards;
        public int Score => _score;
        public int Combo => _combo;
        public int MaxCombo => _maxCombo;
        public int Hp => _hp;
        public bool Night => _night;
        public int Wave => _wave;
        public bool Ended => _ended;

        public void Begin()
        {
            _timeLeft = MatchConfig.MatchSeconds;
            _kukichi = _kokked = _shards = _score = _combo = _maxCombo = _damageTaken = 0;
            _hp = MatchConfig.MaxHp;
            _mirrorFound = _eyeFound = _ended = _night = false;
            _wave = 0;
            _wave1 = _wave2 = _wave4 = _mirrorHint = false;
            _comboTimer = 0f;
            _swampDepth = 0;

            RenderSettings.fogColor = VisualPalette.Fog;
            RenderSettings.fogDensity = 0.028f;
            RenderSettings.ambientLight = VisualPalette.Ambient;

            ClearDynamics();
            SpawnInitial();
            Player.OnKok = DoKok;
            Player.OnDash = () => Cam?.Punch(0.12f);
            OnToast?.Invoke("Сумеречная роща — найди 3 следа пацаноида");
            OnHudDirty?.Invoke();
        }

        public void PlaceSecrets(SecretInteractable mirror, SecretInteractable well)
        {
            mirror.OnActivated = OnSecret;
            well.OnActivated = OnSecret;
        }

        void ClearDynamics()
        {
            foreach (var e in _enemies) if (e) Destroy(e.gameObject);
            _enemies.Clear();
            foreach (var p in _pickups) if (p) Destroy(p.gameObject);
            _pickups.Clear();
        }

        void SpawnInitial()
        {
            for (int i = 0; i < 10; i++) SpawnBobyl(false);
            for (int i = 0; i < 3; i++) SpawnBobyl(true);
            SpawnShard();
            SpawnShard();
            for (int i = 0; i < 22; i++) SpawnKukichi();
            SpawnPowerRandom();
            SpawnPowerRandom();
        }

        void Update()
        {
            if (_ended || Player == null) return;
            _timeLeft -= Time.deltaTime;
            if (_comboTimer > 0f)
            {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0f) _combo = 0;
            }

            TickMilestones();
            MagnetPull();
            ContactDamage();
            if (_timeLeft <= 0f) Finish(_shards >= 3);
            OnHudDirty?.Invoke();
        }

        void TickMilestones()
        {
            float t = _timeLeft;
            if (!_wave1 && t <= 120f) { _wave1 = true; TriggerWave(1, "ВОЛНА 1"); }
            if (!_wave2 && t <= 90f) { _wave2 = true; TriggerWave(2, "ВОЛНА 2"); }
            if (!_night && t <= MatchConfig.NightAtSecondsLeft)
            {
                _night = true;
                RenderSettings.fogColor = Color.Lerp(VisualPalette.Fog, VisualPalette.NightTint, 0.55f);
                RenderSettings.fogDensity = 0.038f;
                RenderSettings.ambientLight = VisualPalette.NightTint * 1.4f;
                SaveService.Write(SaveService.Unlock(SaveService.Load(), "jvachnik"));
                TriggerWave(3, "НОЧНАЯ ВОЛНА");
                OnToast?.Invoke("Ночь. Жвачники вышли на охоту.");
            }
            if (!_mirrorHint && t <= 45f && _shards < 3 && !_mirrorFound)
            {
                _mirrorHint = true;
                OnToast?.Invoke("Зеркало Истукануса где-то в роще…");
            }
            if (!_wave4 && t <= 30f) { _wave4 = true; TriggerWave(4, "ФИНАЛЬНЫЙ НАПЛЫВ"); }
        }

        void TriggerWave(int n, string label)
        {
            _wave = n;
            OnToast?.Invoke(label);
            for (int i = 0; i < 4 + n * 2; i++) SpawnBobyl(false);
            for (int i = 0; i < n; i++) SpawnBobyl(true);
            if (_night)
                for (int i = 0; i < 2 + n; i++) SpawnJvachnik();
        }

        Vector3 FarPoint(float minDist = 12f)
        {
            for (int i = 0; i < 20; i++)
            {
                var p = Random.onUnitSphere;
                p.y = 0f;
                p = p.normalized * Random.Range(8f, MatchConfig.WorldRadius - 3f);
                if (Player == null || Vector3.Distance(p, Player.transform.position) >= minDist)
                    return p;
            }
            return Player.transform.position + Vector3.forward * minDist;
        }

        void SpawnBobyl(bool hard)
        {
            var go = new GameObject(hard ? "BlackEclair" : "Bobyl");
            go.transform.SetParent(WorldRoot, false);
            go.transform.position = FarPoint(hard ? 14f : 12f);
            var e = go.AddComponent<Enemy>();
            e.Init(hard ? MobKind.Hard : MobKind.Bobyl, Player.transform);
            e.OnDied = OnEnemyDied;
            _enemies.Add(e);
        }

        void SpawnJvachnik()
        {
            var go = new GameObject("Jvachnik");
            go.transform.SetParent(WorldRoot, false);
            go.transform.position = FarPoint(16f);
            var e = go.AddComponent<Enemy>();
            e.Init(MobKind.Jvachnik, Player.transform);
            e.OnDied = OnEnemyDied;
            _enemies.Add(e);
        }

        void SpawnKukichi()
        {
            var go = new GameObject("Kukichi");
            go.transform.SetParent(WorldRoot, false);
            go.transform.position = FarPoint(4f);
            var p = go.AddComponent<Pickup>();
            p.InitKukichi();
            p.OnCollected = OnPickup;
            _pickups.Add(p);
        }

        void SpawnShard()
        {
            var go = new GameObject("Shard");
            go.transform.SetParent(WorldRoot, false);
            go.transform.position = FarPoint(15f);
            var p = go.AddComponent<Pickup>();
            p.InitShard();
            p.OnCollected = OnPickup;
            _pickups.Add(p);
        }

        void SpawnPowerRandom()
        {
            var kinds = new[] { PowerKind.Magnet, PowerKind.Rage, PowerKind.Haste, PowerKind.Shield, PowerKind.Feast };
            var kind = kinds[Random.Range(0, kinds.Length)];
            var go = new GameObject("Power_" + kind);
            go.transform.SetParent(WorldRoot, false);
            go.transform.position = FarPoint(10f);
            var p = go.AddComponent<Pickup>();
            p.InitPower(kind);
            p.OnCollected = OnPickup;
            _pickups.Add(p);
        }

        void OnEnemyDied(Enemy e)
        {
            _enemies.Remove(e);
            _kokked++;
            _combo++;
            _maxCombo = Mathf.Max(_maxCombo, _combo);
            _comboTimer = MatchConfig.ComboWindow;
            int mult = 1 + _combo / 5;
            int gain = (e.Kind == MobKind.Hard ? 35 : e.Kind == MobKind.Jvachnik ? 50 : 18) * mult;
            _score += gain;
            if (Random.value < 0.35f) SpawnKukichiNear(e.transform.position);
            if (Random.value < 0.12f) SpawnPowerNear(e.transform.position);
            var save = SaveService.Load();
            if (e.Kind == MobKind.Bobyl) SaveService.Write(SaveService.Unlock(save, "bobyl"));
            if (e.Kind == MobKind.Hard) SaveService.Write(SaveService.Unlock(save, "bobyl_hard"));
            if (e.Kind == MobKind.Jvachnik) SaveService.Write(SaveService.Unlock(save, "jvachnik"));
        }

        void SpawnKukichiNear(Vector3 pos)
        {
            var go = new GameObject("Kukichi");
            go.transform.SetParent(WorldRoot, false);
            var flat = pos + Random.insideUnitSphere * 1.5f;
            flat.y = 0f;
            go.transform.position = flat;
            var p = go.AddComponent<Pickup>();
            p.InitKukichi();
            p.OnCollected = OnPickup;
            _pickups.Add(p);
        }

        void SpawnPowerNear(Vector3 pos)
        {
            var kinds = new[] { PowerKind.Magnet, PowerKind.Rage, PowerKind.Haste, PowerKind.Shield, PowerKind.Feast };
            var kind = kinds[Random.Range(0, kinds.Length)];
            var go = new GameObject("Power_" + kind);
            go.transform.SetParent(WorldRoot, false);
            var flat = pos;
            flat.y = 0f;
            go.transform.position = flat;
            var p = go.AddComponent<Pickup>();
            p.InitPower(kind);
            p.OnCollected = OnPickup;
            _pickups.Add(p);
        }

        void OnPickup(Pickup p)
        {
            _pickups.Remove(p);
            switch (p.Kind)
            {
                case PickupKind.Kukichi:
                    _kukichi++;
                    _score += 5 * (1 + _combo / 8);
                    break;
                case PickupKind.Shard:
                    _shards++;
                    _score += 120;
                    OnToast?.Invoke($"След пропавшего пацаноида {_shards}/3");
                    SaveService.Write(SaveService.Unlock(SaveService.Load(), "pacanoid"));
                    if (_shards >= 3) Finish(true);
                    break;
                case PickupKind.Power:
                    if (p.Power == PowerKind.Feast)
                    {
                        _hp = Mathf.Min(MatchConfig.MaxHp, _hp + 1);
                        _kukichi += 3;
                        OnToast?.Invoke("Пир: +HP и кукичи");
                    }
                    else
                    {
                        Player.ApplyPower(p.Power);
                        OnToast?.Invoke(PowerLabel(p.Power));
                    }
                    _score += 40;
                    break;
            }
        }

        static string PowerLabel(PowerKind k) => k switch
        {
            PowerKind.Magnet => "Магнит кукичей",
            PowerKind.Rage => "Ярость кока",
            PowerKind.Haste => "Ускорение",
            PowerKind.Shield => "Щит",
            _ => "Пауэр-ап"
        };

        void OnSecret(SecretInteractable s)
        {
            if (s.Type == SecretInteractable.SecretType.Mirror)
            {
                _mirrorFound = true;
                if (_shards < 3) SpawnShard();
                SaveService.Write(SaveService.Unlock(SaveService.Load(), "istukanus"));
                OnToast?.Invoke("Истуканус: «Брунявая Чуня или Чунявая Бруня?» — обои.");
                _score += 100;
            }
            else
            {
                _eyeFound = true;
                _hp = Mathf.Min(MatchConfig.MaxHp, _hp + 1);
                _kukichi += 5;
                var save = SaveService.Unlock(SaveService.Load(), "spectral");
                SaveService.Write(SaveService.Unlock(save, "scripach"));
                OnToast?.Invoke("Спектральный колодец: +1 HP и +5 кукичей");
                _score += 80;
            }
        }

        void DoKok()
        {
            Player.PunchVisual();
            Cam?.Punch(0.2f);
            int dmg = Player.HasRage ? 2 : 1;
            float range = MatchConfig.KokRange * (Player.HasRage ? 1.2f : 1f);
            var hitList = new List<Enemy>(_enemies);
            foreach (var e in hitList)
            {
                if (!e) continue;
                var to = e.transform.position - Player.transform.position;
                to.y = 0f;
                if (to.magnitude > range) continue;
                float dot = Vector3.Dot(Player.Facing, to.normalized);
                if (dot < 0.05f && to.magnitude > 1.2f) continue;
                e.ReceiveKok(dmg);
            }
        }

        void MagnetPull()
        {
            if (!Player.HasMagnet) return;
            foreach (var p in _pickups)
            {
                if (!p || p.Kind != PickupKind.Kukichi) continue;
                p.MagnetPull(Player.transform, 14f);
            }
        }

        void ContactDamage()
        {
            if (Player.IsInvulnerable) return;
            foreach (var e in _enemies)
            {
                if (!e || !e.CanContactDamage) continue;
                float dist = Vector3.Distance(
                    new Vector3(Player.transform.position.x, 0f, Player.transform.position.z),
                    new Vector3(e.transform.position.x, 0f, e.transform.position.z));
                float reach = e.Kind == MobKind.Jvachnik ? 1.35f : 1.05f;
                if (dist > reach) continue;
                if (e.Kind == MobKind.Jvachnik) { Hurt(1); return; }
                if (_night) { Hurt(1); return; }
            }
        }

        void Hurt(int amount)
        {
            if (Player.ConsumeShield())
            {
                OnToast?.Invoke("Щит поглотил удар");
                Player.ApplyHurtInvuln();
                return;
            }
            _hp -= amount;
            _damageTaken += amount;
            Player.ApplyHurtInvuln();
            Cam?.Punch(0.35f);
            if (_hp <= 0) Finish(false);
        }

        public void NotifySwamp(bool enter)
        {
            if (enter) _swampDepth++;
            else _swampDepth = Mathf.Max(0, _swampDepth - 1);
            Player.SetSwampSlow(_swampDepth > 0 ? 0.55f : 1f);
        }

        void Finish(bool perfectGoal)
        {
            if (_ended) return;
            _ended = true;
            var result = new MatchResult
            {
                Kukichi = _kukichi,
                Kokked = _kokked,
                Shards = _shards,
                Perfect = perfectGoal && _shards >= 3,
                MirrorFound = _mirrorFound,
                EyeFound = _eyeFound,
                Score = _score + _kukichi * 2 + (_shards >= 3 ? 200 : 0),
                MaxCombo = _maxCombo,
                DamageTaken = _damageTaken
            };
            var save = SaveService.Load();
            save.Kukichi += result.Kukichi;
            save.MatchesPlayed++;
            save.BestScore = Mathf.Max(save.BestScore, result.Score);
            SaveService.Write(SaveService.Unlock(save, "sator"));
            OnFinished?.Invoke(result);
        }
    }
}
