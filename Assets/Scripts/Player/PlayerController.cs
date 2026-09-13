using UnityEngine;
using Zazerkalye.Data;
using Zazerkalye.Visual;

namespace Zazerkalye.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        public Transform VisualRoot;
        public float MoveSpeed = MatchConfig.MoveSpeed;

        CharacterController _cc;
        float _dashLeft, _dashCd, _kokCd, _invuln, _spawnGrace;
        float _magnet, _rage, _haste;
        bool _shield;
        float _swampSlow = 1f;
        Vector3 _facing = Vector3.forward;

        public Vector3 Facing => _facing;
        public bool IsInvulnerable => _invuln > 0f || _dashLeft > 0f || _spawnGrace > 0f || _shield;
        public bool HasMagnet => _magnet > 0f;
        public bool HasRage => _rage > 0f;

        public System.Action OnKok;
        public System.Action OnDash;

        void Awake()
        {
            _cc = GetComponent<CharacterController>();
            _spawnGrace = MatchConfig.SpawnGrace;
            BuildVisual();
        }

        void BuildVisual()
        {
            VisualRoot = new GameObject("Visual").transform;
            VisualRoot.SetParent(transform, false);

            var coat = MeshFactory.Capsule("Coat", new Vector3(0.85f, 0.95f, 0.75f), VisualPalette.PlayerCoat, VisualRoot);
            coat.transform.localPosition = new Vector3(0f, 0.95f, 0f);
            var head = MeshFactory.Sphere("Head", Vector3.one * 0.55f, VisualPalette.PlayerSkin, VisualRoot);
            head.transform.localPosition = new Vector3(0f, 1.85f, 0.05f);
            var hood = MeshFactory.Sphere("Hood", new Vector3(0.62f, 0.45f, 0.62f), VisualPalette.PlayerCoat * 0.85f, VisualRoot);
            hood.transform.localPosition = new Vector3(0f, 2.05f, -0.05f);
            var stick = MeshFactory.Cylinder("Kokalka", new Vector3(0.08f, 0.55f, 0.08f), VisualPalette.Trunk, VisualRoot);
            stick.transform.localPosition = new Vector3(0.55f, 1.1f, 0.35f);
            stick.transform.localRotation = Quaternion.Euler(20f, 0f, -25f);
        }

        void Update()
        {
            float dt = Time.deltaTime;
            Tick(dt);

            Vector2 input = ReadMove();
            Vector3 camF = Camera.main != null
                ? Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized
                : Vector3.forward;
            Vector3 camR = Camera.main != null
                ? Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up).normalized
                : Vector3.right;

            Vector3 wish = camF * input.y + camR * input.x;
            if (wish.sqrMagnitude > 1f) wish.Normalize();

            float speed = MoveSpeed * _swampSlow * (_haste > 0f ? 1.35f : 1f);
            Vector3 vel = _dashLeft > 0f ? _facing * MatchConfig.DashSpeed : wish * speed;
            if (_dashLeft <= 0f && wish.sqrMagnitude > 0.01f)
            {
                _facing = wish.normalized;
                VisualRoot.rotation = Quaternion.Slerp(VisualRoot.rotation, Quaternion.LookRotation(_facing), dt * 12f);
            }
            vel.y = -2f;
            _cc.Move(vel * dt);

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) TryKok();
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) TryDash();
        }

        Vector2 ReadMove()
        {
            float x = 0f, y = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
            var v = new Vector2(x, y);
            return v.sqrMagnitude > 1f ? v.normalized : v;
        }

        void Tick(float dt)
        {
            if (_dashLeft > 0f) _dashLeft -= dt;
            if (_dashCd > 0f) _dashCd -= dt;
            if (_kokCd > 0f) _kokCd -= dt;
            if (_invuln > 0f) _invuln -= dt;
            if (_spawnGrace > 0f) _spawnGrace -= dt;
            if (_magnet > 0f) _magnet -= dt;
            if (_rage > 0f) _rage -= dt;
            if (_haste > 0f) _haste -= dt;
        }

        public void TryKok()
        {
            if (_kokCd > 0f) return;
            _kokCd = MatchConfig.KokCooldown * (_rage > 0f ? 0.65f : 1f);
            OnKok?.Invoke();
        }

        public void TryDash()
        {
            if (_dashCd > 0f || _dashLeft > 0f) return;
            _dashCd = MatchConfig.DashCooldown;
            _dashLeft = MatchConfig.DashDuration;
            OnDash?.Invoke();
        }

        public void ApplyHurtInvuln() => _invuln = MatchConfig.HurtInvuln;
        public void SetSwampSlow(float mul) => _swampSlow = mul;

        public void ApplyPower(PowerKind kind)
        {
            switch (kind)
            {
                case PowerKind.Magnet: _magnet = 8f; break;
                case PowerKind.Rage: _rage = 7f; break;
                case PowerKind.Haste: _haste = 7f; break;
                case PowerKind.Shield: _shield = true; break;
            }
        }

        public bool ConsumeShield()
        {
            if (!_shield) return false;
            _shield = false;
            return true;
        }

        public void PunchVisual()
        {
            if (VisualRoot != null) VisualRoot.localScale = Vector3.one * 1.08f;
        }

        void LateUpdate()
        {
            if (VisualRoot != null)
                VisualRoot.localScale = Vector3.Lerp(VisualRoot.localScale, Vector3.one, Time.deltaTime * 10f);
        }
    }
}
