using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zazerkalye.Player;

namespace Zazerkalye.UI
{
    public class VirtualPad : MonoBehaviour
    {
        public static VirtualPad Create(Transform hudRoot, Font font)
        {
            var go = new GameObject("VirtualPad", typeof(RectTransform));
            go.transform.SetParent(hudRoot, false);
            var root = go.GetComponent<RectTransform>();
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
            var pad = go.AddComponent<VirtualPad>();
            pad.Build(font);
            return pad;
        }

        void Build(Font font)
        {
            var baseRt = Panel("StickBase", new Vector2(0.04f, 0.06f), new Vector2(0.22f, 0.34f), new Color(0f, 0f, 0f, 0.38f));
            var thumbGo = new GameObject("StickThumb", typeof(RectTransform), typeof(Image));
            thumbGo.transform.SetParent(baseRt, false);
            var thumbRt = thumbGo.GetComponent<RectTransform>();
            thumbRt.anchorMin = new Vector2(0.5f, 0.5f);
            thumbRt.anchorMax = new Vector2(0.5f, 0.5f);
            thumbRt.sizeDelta = new Vector2(72f, 72f);
            thumbRt.anchoredPosition = Vector2.zero;
            var thumbImg = thumbGo.GetComponent<Image>();
            thumbImg.color = new Color(0.82f, 0.88f, 0.72f, 0.6f);
            thumbImg.raycastTarget = false;
            var stick = baseRt.gameObject.AddComponent<StickZone>();
            stick.Thumb = thumbRt;

            var kok = MakeButton("Kok", "КОК", new Vector2(0.80f, 0.08f), new Vector2(0.96f, 0.28f), new Color(0.72f, 0.28f, 0.22f, 0.78f), font);
            kok.onClick.AddListener(GameplayInput.QueueKok);
            var dash = MakeButton("Dash", "РЫВ", new Vector2(0.64f, 0.06f), new Vector2(0.78f, 0.20f), new Color(0.22f, 0.38f, 0.55f, 0.78f), font);
            dash.onClick.AddListener(GameplayInput.QueueDash);
        }

        RectTransform Panel(string name, Vector2 min, Vector2 max, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return rt;
        }

        UnityEngine.UI.Button MakeButton(string name, string label, Vector2 min, Vector2 max, Color color, Font font)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(UnityEngine.UI.Button));
            go.transform.SetParent(transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = color;
            var btn = go.GetComponent<UnityEngine.UI.Button>();
            btn.targetGraphic = img;
            var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var trt = textGo.GetComponent<RectTransform>();
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.offsetMin = Vector2.zero;
            trt.offsetMax = Vector2.zero;
            var t = textGo.GetComponent<Text>();
            t.text = label;
            t.alignment = TextAnchor.MiddleCenter;
            t.fontSize = 26;
            t.color = Color.white;
            t.font = font;
            t.raycastTarget = false;
            return btn;
        }

        void OnDisable() => GameplayInput.Stick = Vector2.zero;

        class StickZone : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
        {
            public RectTransform Thumb;

            public void OnPointerDown(PointerEventData eventData) => OnDrag(eventData);

            public void OnDrag(PointerEventData eventData)
            {
                var rt = (RectTransform)transform;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, eventData.pressEventCamera, out var local);
                float radius = Mathf.Max(36f, Mathf.Min(rt.rect.width, rt.rect.height) * 0.45f);
                var clamped = Vector2.ClampMagnitude(local, radius);
                if (Thumb != null) Thumb.anchoredPosition = clamped;
                GameplayInput.Stick = clamped / radius;
            }

            public void OnPointerUp(PointerEventData eventData)
            {
                GameplayInput.Stick = Vector2.zero;
                if (Thumb != null) Thumb.anchoredPosition = Vector2.zero;
            }

            void OnDisable()
            {
                GameplayInput.Stick = Vector2.zero;
                if (Thumb != null) Thumb.anchoredPosition = Vector2.zero;
            }
        }
    }
}
