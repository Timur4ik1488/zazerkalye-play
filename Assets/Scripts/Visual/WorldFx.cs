using UnityEngine;

namespace Zazerkalye.Visual
{
    public class Billboard : MonoBehaviour
    {
        public bool FlattenY = true;

        void LateUpdate()
        {
            var cam = Camera.main;
            if (cam == null) return;
            var to = cam.transform.position - transform.position;
            if (FlattenY) to.y = 0f;
            if (to.sqrMagnitude < 0.0001f) return;
            transform.rotation = Quaternion.LookRotation(-to.normalized, Vector3.up);
        }
    }

    public class FigurineIdle : MonoBehaviour
    {
        public float Bob = 0.04f;
        public float Speed = 1.6f;
        Vector3 _base;

        void Start() => _base = transform.localPosition;

        void Update()
        {
            float t = Time.time * Speed;
            transform.localPosition = _base + Vector3.up * (Mathf.Sin(t) * Bob);
            transform.localRotation = Quaternion.Euler(0f, Mathf.Sin(t * 0.7f) * 6f, 0f);
        }
    }

    public class QuestBeacon : MonoBehaviour
    {
        public Color Color = VisualPalette.Pacanoid;
        Transform _beam;
        Transform _gem;
        Light _light;

        public static QuestBeacon Create(Transform parent)
        {
            var go = new GameObject("QuestBeacon");
            go.transform.SetParent(parent, false);
            var b = go.AddComponent<QuestBeacon>();
            b.Build();
            go.SetActive(false);
            return b;
        }

        void Build()
        {
            _beam = MeshFactory.Cylinder("Beam", new Vector3(0.18f, 3.2f, 0.18f), Color, transform).transform;
            _beam.localPosition = new Vector3(0f, 3.2f, 0f);
            _beam.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.Transparent(new Color(Color.r, Color.g, Color.b, 0.28f));
            _gem = MeshFactory.Sphere("Gem", Vector3.one * 0.55f, Color, transform).transform;
            _gem.localPosition = new Vector3(0f, 6.4f, 0f);
            _gem.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.Glow(Color);
            var lightGo = new GameObject("Light");
            lightGo.transform.SetParent(transform, false);
            lightGo.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            _light = lightGo.AddComponent<Light>();
            _light.type = LightType.Point;
            _light.range = 10f;
            _light.intensity = 2.4f;
            _light.color = Color;
        }

        public void Show(Vector3 worldPos, Color color)
        {
            Color = color;
            gameObject.SetActive(true);
            var p = worldPos;
            p.y = 0f;
            transform.position = p;
            if (_light != null) _light.color = color;
            if (_gem != null)
                _gem.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.Glow(color);
            if (_beam != null)
                _beam.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.Transparent(new Color(color.r, color.g, color.b, 0.28f));
        }

        public void Hide() => gameObject.SetActive(false);

        void Update()
        {
            if (_gem == null) return;
            _gem.Rotate(0f, 80f * Time.deltaTime, 0f);
            float s = 1f + Mathf.Sin(Time.time * 3f) * 0.12f;
            _gem.localScale = Vector3.one * (0.55f * s);
        }
    }
}
