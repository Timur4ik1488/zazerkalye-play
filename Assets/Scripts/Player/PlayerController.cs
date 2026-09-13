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
        public bool IsInvulnerable => _invuln > 0f || _dashLeft > 0f || _spawnGrace > 0f;
        public bool HasMagnet => _magnet > 0f;
        public bool HasRage => _rage > 0f;

        public System.Action OnKok;
        public System.Action OnDash;

        void Awake()
        {
            _cc = GetComponent<CharacterController>();
            BuildVisual();
            ResetForMatch();
        }

        void BuildVisual()
        {
            VisualRoot = new GameObject("Visual").transform;
            VisualRoot.SetParent(transform, false);
            CharacterView.Attach("sator", VisualRoot, 2.15f);
        }

        public void ResetForMatch()
        {
            _dashLeft = _dashCd = _kokCd = _invuln = 0f;
            _magnet = _rage = _haste = 0f;
            _shield = false;
            _swampSlow = 1f;
            _spawnGrace = MatchConfig.SpawnGrace;
            _facing = Vector3.forward;
            if (VisualRoot != null)
            {
                VisualRoot.localRotation = Quaternion.identity;
                VisualRoot.localScale = Vector3.one;
            }
        }

        public void Warp(Vector3 position)
        {
            if (_cc == null) _cc = GetComponent<CharacterController>();
            bool wasEnabled = _cc.enabled;
            _cc.enabled = false;
            transform.SetPositionAndRotation(position, Quaternion.identity);
            _cc.enabled = wasEnabled;
        }

        void Update()
        {
            if (!isActiveAndEnabled) return;
            float dt = Time.deltaTime;
            Tick(dt);

            Vector2 input = ReadMove() + GameplayInput.Stick;
            if (input.sqrMagnitude > 1f) input.Normalize();

            Vector3 camF = Camera.main != null
                ? Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized
                : Vector3.forward;
            Vector3 camR = Camera.main != null
                ? Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up).normalized
                : Vector3.right;
            if (camF.sqrMagnitude < 0.01f) camF = Vector3.forward;
            if (camR.sqrMagnitude < 0.01f) camR = Vector3.right;

            Vector3 wish = camF * input.y + camR * input.x;
            if (wish.sqrMagnitude > 1f) wish.Normalize();

            float speed = MoveSpeed * _swampSlow * (_haste > 0f ? 1.35f : 1f);
            Vector3 vel = _dashLeft > 0f ? _facing * MatchConfig.DashSpeed : wish * speed;
            if (_dashLeft <= 0f && wish.sqrMagnitude > 0.01f)
                _facing = wish.normalized;

            vel.y = 0f;
            _cc.Move(vel * dt);
            ClampToGrove();

            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Space) || GameplayInput.ConsumeKok())
                TryKok();
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift) || GameplayInput.ConsumeDash())
                TryDash();
        }

        void ClampToGrove()
        {
            var p = transform.position;
            p.y = 0f;
            float max = MatchConfig.WorldRadius - 1.2f;
            if (p.sqrMagnitude > max * max)
                p = p.normalized * max;
            if ((transform.position - p).sqrMagnitude > 0.0001f)
            {
                _cc.enabled = false;
                transform.position = p;
                _cc.enabled = true;
            }
            else if (Mathf.Abs(transform.position.y) > 0.001f)
            {
                _cc.enabled = false;
                transform.position = p;
                _cc.enabled = true;
            }
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
            if (VisualRoot == null) return;
            VisualRoot.localScale = Vector3.Lerp(VisualRoot.localScale, Vector3.one, Time.deltaTime * 10f);
            if (_facing.sqrMagnitude > 0.001f)
            {
                var look = Quaternion.LookRotation(_facing, Vector3.up);
                VisualRoot.rotation = Quaternion.Slerp(VisualRoot.rotation, look, 1f - Mathf.Exp(-10f * Time.deltaTime));
            }
        }
    }
}
