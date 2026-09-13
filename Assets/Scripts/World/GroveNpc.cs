using UnityEngine;
using Zazerkalye.Visual;

namespace Zazerkalye.World
{
    public enum NpcLook { Humanoid, Tree, Bone, Mask, Wolf, Shell, Pedal }

    public class GroveNpc : MonoBehaviour
    {
        public string Id;
        public string Line;
        public bool Hazard;
        public bool NightOnly;
        public bool Repeatable;
        public System.Action<GroveNpc> OnTalk;

        bool _used;
        float _pulse;
        float _talkCd;

        public void Init(string id, string line, NpcLook look, bool kneeling = false, bool hazard = false, bool nightOnly = false, bool repeatable = false)
        {
            Id = id;
            Line = line;
            Hazard = hazard;
            NightOnly = nightOnly;
            Repeatable = repeatable;
            switch (look)
            {
                case NpcLook.Tree: BuildTree(kneeling); break;
                case NpcLook.Bone: BuildBone(); break;
                case NpcLook.Mask: BuildMask(); break;
                case NpcLook.Wolf: BuildWolf(); break;
                case NpcLook.Shell: BuildShell(); break;
                case NpcLook.Pedal: BuildPedal(); break;
                default: BuildHumanoid(new Color(0.35f, 0.48f, 0.32f), VisualPalette.PlayerSkin, kneeling); break;
            }
            if (nightOnly) gameObject.SetActive(false);
        }

        public void ResetForMatch()
        {
            _used = false;
            _talkCd = 0f;
            if (NightOnly) gameObject.SetActive(false);
            else gameObject.SetActive(true);
        }

        public void AppearAtNight()
        {
            if (NightOnly) gameObject.SetActive(true);
        }

        public bool TryTalk(Vector3 playerPos, float range = 2.3f)
        {
            if ((!Repeatable && _used) || Hazard || !gameObject.activeInHierarchy) return false;
            if (_talkCd > 0f) return false;
            var d = playerPos - transform.position;
            d.y = 0f;
            if (d.sqrMagnitude > range * range) return false;
            _used = true;
            _talkCd = Repeatable ? 5f : 0f;
            OnTalk?.Invoke(this);
            return true;
        }

        public bool InReach(Vector3 playerPos, float range)
        {
            if (!gameObject.activeInHierarchy) return false;
            var d = playerPos - transform.position;
            d.y = 0f;
            return d.sqrMagnitude <= range * range;
        }

        void BuildHumanoid(Color coat, Color skin, bool kneeling)
        {
            float h = kneeling ? 1.15f : 1.7f;
            var body = MeshFactory.Capsule("Body", new Vector3(0.55f, kneeling ? 0.45f : 0.7f, 0.55f), coat, transform);
            body.transform.localPosition = new Vector3(0f, h * 0.45f, 0f);
            var head = MeshFactory.Sphere("Head", Vector3.one * 0.42f, skin, transform);
            head.transform.localPosition = new Vector3(0f, h * 0.85f, 0.04f);
        }

        void BuildTree(bool kneeling)
        {
            float h = kneeling ? 1.2f : 2.1f;
            var trunk = MeshFactory.Cylinder("Trunk", new Vector3(0.32f, h * 0.45f, 0.32f), VisualPalette.Trunk, transform);
            trunk.transform.localPosition = new Vector3(0f, h * 0.4f, 0f);
            var canopy = MeshFactory.Sphere("Canopy", Vector3.one * (kneeling ? 0.85f : 1.15f), VisualPalette.Canopy, transform);
            canopy.transform.localPosition = new Vector3(0.1f, h * 0.85f, 0f);
        }

        void BuildBone()
        {
            var body = MeshFactory.Capsule("Body", new Vector3(0.5f, 0.55f, 0.5f), VisualPalette.Bone, transform);
            body.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            var head = MeshFactory.Sphere("Head", Vector3.one * 0.4f, VisualPalette.Bone * 0.92f, transform);
            head.transform.localPosition = new Vector3(0f, 1.35f, 0.05f);
            var log = MeshFactory.Cylinder("Polen", new Vector3(0.18f, 0.55f, 0.18f), VisualPalette.Trunk, transform);
            log.transform.localPosition = new Vector3(0.55f, 0.2f, 0.35f);
            log.transform.localRotation = Quaternion.Euler(75f, 20f, 0f);
        }

        void BuildMask()
        {
            var body = MeshFactory.Capsule("Body", new Vector3(0.6f, 0.75f, 0.55f), new Color(0.22f, 0.26f, 0.22f), transform);
            body.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            var mask = MeshFactory.Sphere("Mask", new Vector3(0.5f, 0.55f, 0.22f), new Color(0.45f, 0.32f, 0.18f), transform);
            mask.transform.localPosition = new Vector3(0f, 1.7f, 0.18f);
        }

        void BuildWolf()
        {
            var body = MeshFactory.Capsule("Body", new Vector3(0.7f, 0.55f, 0.85f), new Color(0.28f, 0.22f, 0.30f), transform);
            body.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            var head = MeshFactory.Sphere("Head", new Vector3(0.45f, 0.4f, 0.55f), new Color(0.32f, 0.26f, 0.28f), transform);
            head.transform.localPosition = new Vector3(0f, 1.25f, 0.25f);
        }

        void BuildShell()
        {
            BuildHumanoid(new Color(0.55f, 0.48f, 0.30f), new Color(0.80f, 0.70f, 0.52f), false);
            var shell = MeshFactory.Sphere("Shell", Vector3.one * 0.35f, VisualPalette.Bobyl, transform);
            shell.transform.localPosition = new Vector3(0.45f, 0.9f, 0.2f);
        }

        void BuildPedal()
        {
            BuildHumanoid(new Color(0.45f, 0.28f, 0.22f), VisualPalette.PlayerSkin, false);
            var pedal = MeshFactory.Cylinder("Pedal", new Vector3(0.35f, 0.08f, 0.12f), VisualPalette.Comb, transform);
            pedal.transform.localPosition = new Vector3(0.4f, 0.15f, 0.25f);
        }

        void Update()
        {
            if (_talkCd > 0f) _talkCd -= Time.deltaTime;
            if (Hazard) return;
            _pulse += Time.deltaTime;
            transform.localScale = Vector3.one * (1f + Mathf.Sin(_pulse * 2.2f) * 0.03f);
        }
    }
}
