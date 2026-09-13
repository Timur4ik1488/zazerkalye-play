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

        public void InitKukichi()
        {
            Kind = PickupKind.Kukichi;
            var body = MeshFactory.Sphere("Gem", Vector3.one * 0.35f, VisualPalette.Kukichi, transform);
            body.transform.localPosition = Vector3.up * 0.35f;
            AddTrigger(0.4f);
            _origin = transform.position;
        }

        public void InitShard()
        {
            Kind = PickupKind.Shard;
            var body = MeshFactory.Capsule("Shard", new Vector3(0.35f, 0.55f, 0.35f), VisualPalette.Shard, transform);
            body.transform.localPosition = Vector3.up * 0.7f;
            body.transform.localRotation = Quaternion.Euler(20f, 0f, 35f);
            var glow = MeshFactory.Sphere("Glow", Vector3.one * 0.9f, new Color(0.45f, 0.82f, 0.55f, 0.25f), transform, true);
            glow.transform.localPosition = Vector3.up * 0.7f;
            AddTrigger(0.7f);
            _origin = transform.position;
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
            AddTrigger(0.55f);
            _origin = transform.position;
        }

        void AddTrigger(float radius)
        {
            var col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.center = Vector3.up * 0.5f;
            col.radius = radius;
        }

        void Update()
        {
            _spin += Time.deltaTime * 90f;
            transform.rotation = Quaternion.Euler(0f, _spin, 0f);
            transform.position = _origin + Vector3.up * (Mathf.Sin(Time.time * 2.5f) * 0.12f);
        }

        public void MagnetPull(Transform target, float speed)
        {
            _origin = Vector3.MoveTowards(_origin, target.position, speed * Time.deltaTime);
        }

        void OnTriggerEnter(Collider other)
        {
            if (_taken) return;
            if (other.GetComponentInParent<Zazerkalye.Player.PlayerController>() == null) return;
            _taken = true;
            OnCollected?.Invoke(this);
            Destroy(gameObject);
        }
    }

    public class SecretInteractable : MonoBehaviour
    {
        public enum SecretType { Mirror, Well }
        public SecretType Type;
        public System.Action<SecretInteractable> OnActivated;
        bool _used;
        float _pulse;

        public void InitMirror()
        {
            Type = SecretType.Mirror;
            var frame = MeshFactory.Cylinder("Frame", new Vector3(0.15f, 1.2f, 0.15f), VisualPalette.Trunk, transform);
            frame.transform.localPosition = Vector3.up * 1.2f;
            var glass = MeshFactory.Plane("Glass", new Vector3(0.18f, 1f, 0.28f), VisualPalette.Mirror, transform);
            glass.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            glass.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            glass.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.Transparent(new Color(0.55f, 0.72f, 0.78f, 0.55f));
            var col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.center = Vector3.up * 1f;
            col.radius = 1.4f;
        }

        public void InitWell()
        {
            Type = SecretType.Well;
            var ring = MeshFactory.Cylinder("Ring", new Vector3(1.1f, 0.25f, 1.1f), VisualPalette.Well, transform);
            ring.transform.localPosition = Vector3.up * 0.25f;
            var water = MeshFactory.Cylinder("Water", new Vector3(0.85f, 0.05f, 0.85f), new Color(0.25f, 0.35f, 0.55f, 0.7f), transform);
            water.transform.localPosition = Vector3.up * 0.35f;
            water.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.Transparent(new Color(0.25f, 0.35f, 0.55f, 0.7f));
            var col = gameObject.AddComponent<SphereCollider>();
            col.isTrigger = true;
            col.center = Vector3.up * 0.5f;
            col.radius = 1.5f;
        }

        void Update()
        {
            _pulse += Time.deltaTime;
            transform.localScale = Vector3.one * (1f + Mathf.Sin(_pulse * 2f) * 0.02f);
        }

        void OnTriggerEnter(Collider other)
        {
            if (_used) return;
            if (other.GetComponentInParent<Zazerkalye.Player.PlayerController>() == null) return;
            _used = true;
            OnActivated?.Invoke(this);
        }
    }
}
