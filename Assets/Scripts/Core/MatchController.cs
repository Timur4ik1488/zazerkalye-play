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
        public GroveBuilder Grove;
        public Light KeyLight;
        public System.Action<MatchResult> OnFinished;
        public System.Action<string> OnToast;
        public System.Action OnHudDirty;

        readonly List<Enemy> _enemies = new();
        readonly List<Pickup> _pickups = new();
        readonly List<GroveNpc> _npcs = new();
        SecretInteractable _mirror;
        SecretInteractable _well;
        QuestBeacon _beacon;

        float _timeLeft = MatchConfig.MatchSeconds;
        int _kukichi, _kokked, _shards, _score, _combo, _maxCombo;
        float _comboTimer;
        int _hp = MatchConfig.MaxHp;
        int _damageTaken;
        bool _mirrorFound, _eyeFound, _ended, _night;
        int _wave;
        bool _wave1, _wave2, _wave4, _mirrorHint, _introHint;
        float _introHintAt;
        Color _dayKeyColor;
        float _dayKeyIntensity = 1.15f;

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
        public string QuestTitle { get; private set; } = "";
        public string QuestHint { get; private set; } = "";
        public string InteractHint { get; private set; } = "";
        public Vector3 QuestWorld { get; private set; }
        public bool QuestHasPoint { get; private set; }

        public void PlaceWorld(SecretInteractable mirror, SecretInteractable well, IEnumerable<GroveNpc> npcs)
        {
            _mirror = mirror;
            _well = well;
            _npcs.Clear();
            if (npcs != null) _npcs.AddRange(npcs);
            WireInteractables();
            if (_beacon == null && WorldRoot != null)
                _beacon = QuestBeacon.Create(WorldRoot);
            RefreshQuest();
        }

        void WireInteractables()
        {
            if (_mirror != null) _mirror.OnActivated = OnSecret;
            if (_well != null) _well.OnActivated = OnSecret;
            foreach (var npc in _npcs)
                if (npc != null) npc.OnTalk = OnNpc;
        }

        public void Begin()
        {
            _timeLeft = MatchConfig.MatchSeconds;
            _kukichi = _kokked = _shards = _score = _combo = _maxCombo = _damageTaken = 0;
            _hp = MatchConfig.MaxHp;
            _mirrorFound = _eyeFound = _ended = _night = false;
            _wave = 0;
            _wave1 = _wave2 = _wave4 = _mirrorHint = false;
            _introHint = false;
            _introHintAt = Time.unscaledTime + 7f;
            _comboTimer = 0f;

            RenderSettings.fogColor = VisualPalette.Fog;
            RenderSettings.fogDensity = 0.028f;
            RenderSettings.ambientLight = VisualPalette.Ambient;
            if (KeyLight != null)
            {
                if (_dayKeyColor == default)
                {
                    _dayKeyColor = KeyLight.color;
                    _dayKeyIntensity = KeyLight.intensity;
                }
                KeyLight.color = VisualPalette.KeyLight;
                KeyLight.intensity = _dayKeyIntensity;
            }

            WireInteractables();
            _mirror?.ResetForMatch();
            _well?.ResetForMatch();
            foreach (var npc in _npcs) npc?.ResetForMatch();

            ClearDynamics();
            SpawnInitial();
            Player.OnKok = DoKok;
            Player.OnDash = () => Cam?.Punch(0.12f);

            var save = SaveService.Load();
            save = SaveService.Unlock(save, "sator");
            save = SaveService.Unlock(save, "bobyl");
            save = SaveService.Unlock(save, "polenych");
            SaveService.Write(save);

            OnToast?.Invoke("Задание: собери 3 зелёных следа. Стрелка внизу показывает, куда идти.");
            RefreshQuest();
            OnHudDirty?.Invoke();
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
            if (_ended || Player == null || !isActiveAndEnabled) return;
            _timeLeft -= Time.deltaTime;
            if (_comboTimer > 0f)
            {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0f) _combo = 0;
            }

            if (!_introHint && Time.unscaledTime >= _introHintAt)
            {
                _introHint = true;
                OnToast?.Invoke("F — кокни бобыля перед собой расчёской. Не обязательно, но даёт кукичи и очки.");
            }

            TickMilestones();
            MagnetPull();
            CollectNearby();
            UpdateSwamp();
            ContactDamage();
            RefreshQuest();
            if (_timeLeft <= 0f) Finish(_shards >= 3);
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
                if (KeyLight != null)
                {
                    KeyLight.color = Color.Lerp(VisualPalette.KeyLight, VisualPalette.FillLight, 0.55f);
                    KeyLight.intensity = 0.55f;
                }
                SaveService.Write(SaveService.Unlock(SaveService.Load(), "jvachnik"));
                foreach (var npc in _npcs) npc?.AppearAtNight();
                TriggerWave(3, "НОЧНАЯ ВОЛНА");
                OnToast?.Invoke("Ночь. Жвачники кусаются — не стой рядом. Акакий (подпись над ним) пустит переждать.");
            }
            if (!_mirrorHint && t <= 45f && _shards < 3 && !_mirrorFound)
            {
                _mirrorHint = true;
                OnToast?.Invoke("Два следа есть. Третий откроет каменный идол Истуканус — иди к жёлтой стрелке.");
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
            _kukichi += Mathf.Max(1, 1 + _combo / 5);
            if (Random.value < 0.35f) SpawnKukichiNear(e.transform.position);
            if (Random.value < 0.12f) SpawnPowerNear(e.transform.position);
            var save = SaveService.Load();
            if (e.Kind == MobKind.Bobyl) save = SaveService.Unlock(save, "bobyl");
            if (e.Kind == MobKind.Hard) save = SaveService.Unlock(save, "bobyl_hard");
            if (e.Kind == MobKind.Jvachnik) save = SaveService.Unlock(save, "jvachnik");
            if (Random.value < 0.12f)
            {
                save = SaveService.Unlock(save, "mihail");
                _kukichi += 3;
                OnToast?.Invoke("Михаил забрал скорлупку. +3 кукича");
            }
            SaveService.Write(save);
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
                    OnToast?.Invoke($"След {_shards}/3. " + (_shards < 2
                        ? "Ищи следующее зелёное свечение."
                        : _shards == 2
                            ? "Теперь подойди к каменному идолу Истуканусу."
                            : "Все три! Роща отпускает."));
                    SaveService.Write(SaveService.Unlock(SaveService.Load(), "pacanoid"));
                    if (_shards >= 3) Finish(true);
                    break;
                case PickupKind.Power:
                    if (p.Power == PowerKind.Feast)
                    {
                        _hp = Mathf.Min(MatchConfig.MaxHp, _hp + 1);
                        _kukichi += 8;
                        OnToast?.Invoke("Пир дядюшки: алмазные соления вовремя.");
                        SaveService.Write(SaveService.Unlock(SaveService.Load(), "fantasmagor"));
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
            PowerKind.Shield => "Щит на один удар",
            _ => "Пауэр-ап"
        };

        void OnSecret(SecretInteractable s)
        {
            if (s.Type == SecretInteractable.SecretType.Mirror)
            {
                _mirrorFound = true;
                if (_shards < 3) SpawnShard();
                SaveService.Write(SaveService.Unlock(SaveService.Load(), "istukanus"));
                OnToast?.Invoke("Истуканус открыл третий след. Забери зелёное свечение — и победишь.");
                _score += 100;
            }
            else
            {
                _eyeFound = true;
                _hp = Mathf.Min(MatchConfig.MaxHp, _hp + 1);
                _kukichi += 5;
                var save = SaveService.Unlock(SaveService.Load(), "spectral");
                SaveService.Write(save);
                OnToast?.Invoke("Спектральный колодец под тем же деревом: +1 HP и +5 кукичей");
                _score += 80;
            }
        }

        void OnNpc(GroveNpc npc)
        {
            OnToast?.Invoke(npc.Line);
            if (!string.IsNullOrEmpty(npc.Id))
                SaveService.Write(SaveService.Unlock(SaveService.Load(), npc.Id));
            if (npc.Id == "pedal")
                Player.ApplyPower(PowerKind.Haste);
            if (npc.Id == "akaky")
            {
                Player.ApplyPower(PowerKind.Haste);
                if (_night)
                {
                    _hp = Mathf.Min(MatchConfig.MaxHp, _hp + 1);
                    Player.ApplyHurtInvuln();
                    OnToast?.Invoke("Акакий пустил переждать ночь. Пыльца и +HP.");
                }
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
                float dist = to.magnitude;
                float dot = dist < 0.05f ? 1f : Vector3.Dot(Player.Facing, to.normalized);
                bool behind = dot < -0.2f;
                if (dist > 1.5f && dot < 0.2f) continue;
                int hitDmg = dmg;
                if (e.Kind == MobKind.Hard && behind) hitDmg = Mathf.Max(hitDmg, 2);
                e.ReceiveKok(hitDmg);
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

        void CollectNearby()
        {
            var pos = Player.transform.position;
            for (int i = _pickups.Count - 1; i >= 0; i--)
            {
                var p = _pickups[i];
                if (!p) { _pickups.RemoveAt(i); continue; }
                p.TryCollect(pos);
            }
            _mirror?.TryActivate(pos);
            _well?.TryActivate(pos);
            foreach (var npc in _npcs)
                npc?.TryTalk(pos);
        }

        void UpdateSwamp()
        {
            bool inSwamp = Grove != null && Grove.IsInSwamp(Player.transform.position);
            Player.SetSwampSlow(inSwamp ? 0.55f : 1f);
        }

        void RefreshQuest()
        {
            InteractHint = "";
            if (Player == null)
            {
                QuestHasPoint = false;
                return;
            }
            var pos = Player.transform.position;
            Pickup nearest = null;
            float best = float.MaxValue;
            foreach (var p in _pickups)
            {
                if (!p || p.Kind != PickupKind.Shard) continue;
                float d = (p.transform.position - pos).sqrMagnitude;
                if (d < best) { best = d; nearest = p; }
            }

            QuestTitle = $"Собери 3 следа пацаноида   {_shards}/3";
            if (nearest != null)
            {
                QuestHint = _shards == 0
                    ? "Иди к зелёному столбу света. Парные зелёные головы — это след. Подбери его."
                    : _shards == 1
                        ? "Ещё один след на поляне — снова к зелёному свечению."
                        : "Третий след открыт. Забери его — и заход выигран.";
                QuestWorld = nearest.transform.position;
                QuestHasPoint = true;
                if (best < 16f) InteractHint = "Подойди вплотную — след возьмётся сам";
                _beacon?.Show(QuestWorld, VisualPalette.Pacanoid);
            }
            else if (_shards < 3 && _mirror != null && !_mirrorFound)
            {
                QuestHint = "Следов на поляне больше нет. Подойди к каменному идолу Истуканусу — он откроет третий.";
                QuestWorld = _mirror.transform.position;
                QuestHasPoint = true;
                float d = Xz(pos, _mirror.transform.position);
                if (d < 5f) InteractHint = "Подойди к идолу вплотную";
                _beacon?.Show(QuestWorld, VisualPalette.UiAccent);
            }
            else if (_shards < 3)
            {
                QuestHint = "Истуканус открыл след. Ищи новое зелёное свечение.";
                QuestHasPoint = false;
                _beacon?.Hide();
            }
            else
            {
                QuestTitle = "Следы собраны";
                QuestHint = "Роща отпускает.";
                QuestHasPoint = false;
                _beacon?.Hide();
            }

            if (_night && _shards < 3)
                QuestHint += "  Ночь: не стой у жвачников. Shift — рывок. Акакий укроет.";

            foreach (var npc in _npcs)
            {
                if (npc == null || !npc.gameObject.activeInHierarchy) continue;
                if (npc.Hazard && npc.InReach(pos, 5f))
                    InteractHint = "Коленыч бьёт при касании — Shift, беги зигзагом";
                else if (!npc.Hazard && npc.InReach(pos, 3.2f) && InteractHint.Length == 0)
                    InteractHint = "Поговорить: " + NpcTitle(npc.Id);
            }
        }

        static string NpcTitle(string id) => id switch
        {
            "polenych" => "Поленыч",
            "akaky" => "Акакий",
            "kazimir" => "Казимир",
            "mihail" => "Михаил",
            "pedal" => "Мальчик-педаль",
            "scripach" => "Скрипач",
            _ => id
        };

        static float Xz(Vector3 a, Vector3 b)
        {
            a.y = b.y = 0f;
            return Vector3.Distance(a, b);
        }

        void ContactDamage()
        {
            if (Player.IsInvulnerable) return;
            var pos = Player.transform.position;
            foreach (var npc in _npcs)
            {
                if (npc == null || !npc.Hazard) continue;
                if (!npc.InReach(pos, 1.8f)) continue;
                SaveService.Write(SaveService.Unlock(SaveService.Load(), npc.Id));
                OnToast?.Invoke(npc.Line);
                Hurt(1);
                return;
            }
            foreach (var e in _enemies)
            {
                if (!e || !e.CanContactDamage) continue;
                float dist = Vector3.Distance(
                    new Vector3(pos.x, 0f, pos.z),
                    new Vector3(e.transform.position.x, 0f, e.transform.position.z));
                float reach = e.Kind == MobKind.Jvachnik ? 1.35f : 1.05f;
                if (dist > reach) continue;
                if (e.Kind == MobKind.Jvachnik) { Hurt(1); return; }
                if (_night && Grove != null && Grove.IsInSwamp(e.transform.position)) { Hurt(1); return; }
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
            _combo = 0;
            _comboTimer = 0f;
            Player.ApplyHurtInvuln();
            Cam?.Punch(0.35f);
            if (_hp <= 0) Finish(false);
        }

        void Finish(bool perfectGoal)
        {
            if (_ended) return;
            _ended = true;
            _beacon?.Hide();
            bool traces = perfectGoal && _shards >= 3;
            var result = new MatchResult
            {
                Kukichi = _kukichi,
                Kokked = _kokked,
                Shards = _shards,
                Perfect = traces && _damageTaken == 0,
                MirrorFound = _mirrorFound,
                EyeFound = _eyeFound,
                Score = _score + _kukichi * 2 + (traces ? 200 : 0),
                MaxCombo = _maxCombo,
                DamageTaken = _damageTaken
            };
            var save = SaveService.Load();
            save = SaveService.AddKukichi(save, result.Kukichi);
            save.MatchesPlayed++;
            save.BestScore = Mathf.Max(save.BestScore, result.Score);
            if (result.Perfect) save = SaveService.Unlock(save, "fantasmagor");
            SaveService.Write(SaveService.Unlock(save, "sator"));
            OnFinished?.Invoke(result);
        }

        void OnEnable()
        {
            if (_mirror != null) _mirror.OnActivated = OnSecret;
            if (_well != null) _well.OnActivated = OnSecret;
            foreach (var npc in _npcs)
                if (npc != null) npc.OnTalk = OnNpc;
        }
    }
}
