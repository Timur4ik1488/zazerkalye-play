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
            _ = look;
            string tex = id switch
            {
                "polenych" => "polenych",
                "kolenych" => "kolenych",
                "akaky" => "akaky",
                "kazimir" => "kazimir",
                "mihail" => "mihail",
                "pedal" => "pedal",
                "scripach" => "scripach",
                _ => "akaky"
            };
            float h = kneeling ? 1.7f : id == "kolenych" || id == "scripach" ? 2.15f : 2.0f;
            CharacterView.Attach(tex, transform, h);
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

        void Update()
        {
            if (_talkCd > 0f) _talkCd -= Time.deltaTime;
            if (Hazard) return;
            _pulse += Time.deltaTime;
            transform.localScale = Vector3.one * (1f + Mathf.Sin(_pulse * 2.2f) * 0.03f);
        }
    }
}
