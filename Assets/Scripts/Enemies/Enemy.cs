using UnityEngine;
using Zazerkalye.Data;
using Zazerkalye.Visual;

namespace Zazerkalye.Enemies
{
    public class Enemy : MonoBehaviour
    {
        public MobKind Kind;
        public int Hp = 1;
        public float Speed = 2.4f;
        public Transform Target;
        public System.Action<Enemy> OnDied;

        float _spawnIFrames = 1.2f;
        float _hitFlash;
        Renderer[] _renderers;
        Color[] _baseColors;
        Vector3 _wanderDir;
        float _wanderTimer;
        float _bob;

        public bool CanContactDamage => _spawnIFrames <= 0f;

        public void Init(MobKind kind, Transform target)
        {
            Kind = kind;
            Target = target;
            switch (kind)
            {
                case MobKind.Bobyl:
                    Hp = 1; Speed = Random.Range(2.0f, 2.8f);
                    BuildBobyl(VisualPalette.Bobyl, 1f); break;
                case MobKind.Hard:
                    Hp = 2; Speed = Random.Range(1.7f, 2.2f);
                    BuildBobyl(VisualPalette.HardBobyl, 1.15f); break;
                case MobKind.Jvachnik:
                    Hp = 1; Speed = Random.Range(3.4f, 4.2f);
                    BuildJvachnik(); break;
            }
            _renderers = GetComponentsInChildren<Renderer>();
            _baseColors = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
                _baseColors[i] = ReadColor(_renderers[i]);
        }

        void BuildBobyl(Color color, float scale)
        {
            var body = MeshFactory.Sphere("Body", Vector3.one * 0.95f * scale, color, transform);
            body.transform.localPosition = new Vector3(0f, 0.55f * scale, 0f);
        }

        void BuildJvachnik()
        {
            var body = MeshFactory.Capsule("Body", new Vector3(0.9f, 0.7f, 0.9f), VisualPalette.Jvachnik, transform);
            body.transform.localPosition = new Vector3(0f, 0.85f, 0f);
        }

        void Update()
        {
            if (_spawnIFrames > 0f) _spawnIFrames -= Time.deltaTime;
            if (_hitFlash > 0f)
            {
                _hitFlash -= Time.deltaTime;
                Tint(Color.white);
                if (_hitFlash <= 0f) RestoreTint();
            }
            if (Target == null) return;

            Vector3 to = Target.position - transform.position; to.y = 0f;
            Vector3 dir;
            if (Kind == MobKind.Jvachnik || to.magnitude < 14f)
                dir = to.sqrMagnitude > 0.01f ? to.normalized : transform.forward;
            else
            {
                _wanderTimer -= Time.deltaTime;
                if (_wanderTimer <= 0f)
                {
                    _wanderTimer = Random.Range(1.2f, 2.5f);
                    _wanderDir = Random.onUnitSphere; _wanderDir.y = 0f; _wanderDir.Normalize();
                }
                dir = _wanderDir;
            }
            transform.position += dir * Speed * Time.deltaTime;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 6f);

            _bob += Time.deltaTime * 4f;
            var pos = transform.position;
            pos.y = Mathf.Abs(Mathf.Sin(_bob)) * 0.08f;
            var flat = pos; flat.y = 0f;
            if (flat.magnitude > MatchConfig.WorldRadius - 1f)
            {
                flat = flat.normalized * (MatchConfig.WorldRadius - 1f);
                pos = new Vector3(flat.x, pos.y, flat.z);
            }
            transform.position = pos;
        }

        static Color ReadColor(Renderer r)
        {
            if (r.material.HasProperty("_BaseColor")) return r.material.GetColor("_BaseColor");
            if (r.material.HasProperty("_Color")) return r.material.GetColor("_Color");
            return Color.white;
        }

        void Tint(Color c)
        {
            if (_renderers == null) return;
            foreach (var r in _renderers)
            {
                if (r.material.HasProperty("_BaseColor")) r.material.SetColor("_BaseColor", c);
                else if (r.material.HasProperty("_Color")) r.material.SetColor("_Color", c);
            }
        }

        void RestoreTint()
        {
            if (_renderers == null || _baseColors == null) return;
            for (int i = 0; i < _renderers.Length; i++)
            {
                var r = _renderers[i];
                if (!r) continue;
                var c = _baseColors[i];
                if (r.material.HasProperty("_BaseColor")) r.material.SetColor("_BaseColor", c);
                else if (r.material.HasProperty("_Color")) r.material.SetColor("_Color", c);
            }
        }

        public bool ReceiveKok(int damage = 1)
        {
            Hp -= damage;
            _hitFlash = 0.2f;
            Tint(Color.white);
            if (Hp <= 0) { OnDied?.Invoke(this); Destroy(gameObject); return true; }
            return false;
        }
    }
}
