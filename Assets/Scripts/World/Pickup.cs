using UnityEngine;
using Zazerkalye.Data;
using Zazerkalye.Visual;

namespace Zazerkalye.World
{
    public enum PickupKind { Kukichi, Shard, Power }

    public class Pickup : MonoBehaviour
    {
        public PickupKind Kind;
        public PowerKind Power;
        public System.Action<Pickup> OnCollected;

        float _spin;
        Vector3 _origin;
        bool _taken;
        float _radius = 0.7f;

        public void InitKukichi()
        {
            Kind = PickupKind.Kukichi;
            var body = MeshFactory.Sphere("Core", new Vector3(0.32f, 0.28f, 0.32f), VisualPalette.Kukichi, transform);
            body.transform.localPosition = Vector3.up * 0.32f;
            var bite = MeshFactory.Sphere("Bite", new Vector3(0.18f, 0.16f, 0.18f), VisualPalette.KukichiBite, transform);
            bite.transform.localPosition = new Vector3(0.12f, 0.38f, 0.05f);
            _radius = 0.9f;
            _origin = transform.position;
            _origin.y = 0f;
        }

        public void InitShard()
        {
            Kind = PickupKind.Shard;
            var a = MeshFactory.Sphere("PacanoidA", Vector3.one * 0.48f, VisualPalette.Pacanoid, transform);
            a.transform.localPosition = new Vector3(-0.28f, 0.7f, 0f);
            var b = MeshFactory.Sphere("PacanoidB", Vector3.one * 0.48f, VisualPalette.Pacanoid * 1.08f, transform);
            b.transform.localPosition = new Vector3(0.28f, 0.7f, 0f);
            var glow = MeshFactory.Sphere("Glow", Vector3.one * 1.25f, new Color(0.45f, 0.82f, 0.55f, 0.22f), transform, true);
            glow.transform.localPosition = Vector3.up * 0.7f;
            _radius = 1.25f;
            _origin = transform.position;
            _origin.y = 0f;
        }

        public void InitPower(PowerKind power)
        {
            Kind = PickupKind.Power;
            Power = power;
            Color c = power switch
            {
                PowerKind.Magnet => new Color(0.55f, 0.75f, 0.95f),
                PowerKind.Rage => new Color(0.90f, 0.35f, 0.28f),
                PowerKind.Haste => new Color(0.95f, 0.85f, 0.35f),
                PowerKind.Shield => new Color(0.45f, 0.70f, 0.95f),
                _ => new Color(0.85f, 0.55f, 0.75f)
            };
            var body = MeshFactory.Cylinder("Power", new Vector3(0.45f, 0.2f, 0.45f), c, transform);
            body.transform.localPosition = Vector3.up * 0.5f;
            _radius = 1.0f;
            _origin = transform.position;
            _origin.y = 0f;
        }

        void Update()
        {
            _spin += Time.deltaTime * 90f;
            transform.rotation = Quaternion.Euler(0f, _spin, 0f);
            transform.position = _origin + Vector3.up * (0.35f + Mathf.Sin(Time.time * 2.5f) * 0.12f);
        }

        public void MagnetPull(Transform target, float speed)
        {
            var goal = target.position;
            goal.y = 0f;
            _origin = Vector3.MoveTowards(_origin, goal, speed * Time.deltaTime);
        }

        public bool TryCollect(Vector3 playerPos)
        {
            if (_taken) return false;
            var a = new Vector3(playerPos.x, 0f, playerPos.z);
            var b = new Vector3(_origin.x, 0f, _origin.z);
            if ((a - b).sqrMagnitude > _radius * _radius) return false;
            _taken = true;
            OnCollected?.Invoke(this);
            Destroy(gameObject);
            return true;
        }
    }

    public class SecretInteractable : MonoBehaviour
    {
        public enum SecretType { Mirror, Well }
        public SecretType Type;
        public System.Action<SecretInteractable> OnActivated;
        bool _used;
        float _pulse;

        public void InitIdol()
        {
            Type = SecretType.Mirror;
            var plinth = MeshFactory.Cylinder("Plinth", new Vector3(0.7f, 0.25f, 0.7f), VisualPalette.Idol * 0.8f, transform);
            plinth.transform.localPosition = Vector3.up * 0.25f;
            var body = MeshFactory.Cylinder("Idol", new Vector3(0.55f, 1.1f, 0.55f), VisualPalette.Idol, transform);
            body.transform.localPosition = Vector3.up * 1.35f;
            var head = MeshFactory.Sphere("Head", Vector3.one * 0.7f, VisualPalette.Idol * 1.1f, transform);
            head.transform.localPosition = new Vector3(0f, 2.55f, 0.05f);
            var plaque = MeshFactory.Plane("Riddle", new Vector3(0.12f, 1f, 0.18f), VisualPalette.Mirror, transform);
            plaque.transform.localPosition = new Vector3(0f, 1.5f, 0.55f);
            plaque.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        public void InitWell()
        {
            Type = SecretType.Well;
            var ring = MeshFactory.Cylinder("Ring", new Vector3(1.1f, 0.25f, 1.1f), VisualPalette.Well, transform);
            ring.transform.localPosition = Vector3.up * 0.25f;
            var water = MeshFactory.Cylinder("Water", new Vector3(0.85f, 0.05f, 0.85f), new Color(0.25f, 0.35f, 0.55f, 0.7f), transform);
            water.transform.localPosition = Vector3.up * 0.35f;
            water.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.Transparent(new Color(0.25f, 0.35f, 0.55f, 0.7f));
        }

        public void ResetForMatch() => _used = false;

        public bool TryActivate(Vector3 playerPos, float range = 2.4f)
        {
            if (_used) return false;
            var d = playerPos - transform.position;
            d.y = 0f;
            if (d.sqrMagnitude > range * range) return false;
            _used = true;
            OnActivated?.Invoke(this);
            return true;
        }

        void Update()
        {
            _pulse += Time.deltaTime;
            transform.localScale = Vector3.one * (1f + Mathf.Sin(_pulse * 2f) * 0.02f);
        }
    }
}
