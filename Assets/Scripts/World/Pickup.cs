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

        Vector3 _origin;
        bool _taken;
        float _radius = 0.7f;

        public void InitKukichi()
        {
            Kind = PickupKind.Kukichi;
            CharacterView.Attach("kukichi", transform, 0.7f);
            _radius = 0.9f;
            _origin = transform.position;
            _origin.y = 0f;
        }

        public void InitShard()
        {
            Kind = PickupKind.Shard;
            CharacterView.Attach("pacanoid", transform, 1.15f);
            _radius = 1.25f;
            _origin = transform.position;
            _origin.y = 0f;
        }

        public void InitPower(PowerKind power)
        {
            Kind = PickupKind.Power;
            Power = power;
            CharacterView.Attach("power", transform, 0.85f);
            _radius = 1.0f;
            _origin = transform.position;
            _origin.y = 0f;
        }

        void Update()
        {
            transform.position = _origin + Vector3.up * (0.2f + Mathf.Sin(Time.time * 2.5f) * 0.12f);
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
            CharacterView.Attach("istukanus", transform, 2.7f);
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
