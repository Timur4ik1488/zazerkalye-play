using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Zazerkalye.Core;
using Zazerkalye.Data;
using Zazerkalye.Visual;
using Zazerkalye.Yandex;

namespace Zazerkalye.UI
{
    public class GameUI : MonoBehaviour
    {
        Canvas _canvas;
        Font _font;

        GameObject _menu;
        GameObject _hud;
        GameObject _results;
        GameObject _bestiary;

        Text _hudStats;
        Text _hudGoal;
        Text _toast;
        Text _resultsBody;
        Text _bestiaryBody;

        MatchController _match;
        MatchResult _lastResult;
        float _toastUntil;

        public System.Action OnStartMatch;
        public System.Action OnBackToMenu;

        public void Build(MatchController match)
        {
            _match = match;
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            var canvasGo = new GameObject("UICanvas");
            canvasGo.transform.SetParent(transform, false);
            _canvas = canvasGo.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }

            _menu = BuildMenu();
            _hud = BuildHud();
            _results = BuildResults();
            _bestiary = BuildBestiary();
            ShowMenu();
        }

        public void ShowMenu()
        {
            SetOnly(_menu);
        }

        public void ShowHud()
        {
            SetOnly(_hud);
            RefreshHud();
        }

        public void ShowResults(MatchResult result)
        {
            _lastResult = result;
            SetOnly(_results);
            var sb = new StringBuilder();
            sb.AppendLine(result.Perfect ? "Следы собраны. Роща отпускает." : "Сессия окончена.");
            sb.AppendLine();
            sb.AppendLine($"Счёт: {result.Score}");
            sb.AppendLine($"Кукичи: {result.Kukichi}");
            sb.AppendLine($"Кокнуто: {result.Kokked}");
            sb.AppendLine($"Следы: {result.Shards}/3");
            sb.AppendLine($"Макс. комбо: {result.MaxCombo}");
            if (result.MirrorFound) sb.AppendLine("Истуканус найден.");
            if (result.EyeFound) sb.AppendLine("Спектральный колодец открыт.");
            _resultsBody.text = sb.ToString();
        }

        public void ShowBestiary()
        {
            SetOnly(_bestiary);
            var save = SaveService.Load();
            var sb = new StringBuilder();
            foreach (var e in BestiaryCatalog.All)
            {
                bool unlocked = save.Bestiary.Contains(e.Id);
                sb.AppendLine(unlocked ? $"• {e.Name}" : "• ???");
                if (unlocked) sb.AppendLine($"  {e.Blurb}");
                sb.AppendLine();
            }
            _bestiaryBody.text = sb.ToString();
        }

        public void Toast(string msg)
        {
            if (_toast == null) return;
            _toast.text = msg;
            _toastUntil = Time.unscaledTime + 2.4f;
        }

        public void RefreshHud()
        {
            if (_match == null || _hudStats == null) return;
            int secs = Mathf.CeilToInt(_match.TimeLeft);
            string hearts = new string('♥', Mathf.Max(0, _match.Hp)) + new string('♡', Mathf.Max(0, MatchConfig.MaxHp - _match.Hp));
            _hudStats.text =
                $"{hearts}   {secs / 60:0}:{secs % 60:00}\n" +
                $"Счёт { _match.Score}   Кукичи {_match.Kukichi}   Следы {_match.Shards}/3\n" +
                (_match.Combo > 1 ? $"Комбо ×{_match.Combo}" : "");
            _hudGoal.text = _match.Night
                ? "Ночь в Сумеречной роще — жвачники рядом"
                : "Собери 3 следа · кокай бобылей · не увязни в болоте";
            if (_toast != null && Time.unscaledTime > _toastUntil)
                _toast.text = "";
        }

        void Update()
        {
            if (_hud != null && _hud.activeSelf)
                RefreshHud();
        }

        void SetOnly(GameObject panel)
        {
            _menu.SetActive(panel == _menu);
            _hud.SetActive(panel == _hud);
            _results.SetActive(panel == _results);
            _bestiary.SetActive(panel == _bestiary);
        }

        GameObject BuildMenu()
        {
            var root = Panel("Menu", new Color(0.04f, 0.06f, 0.07f, 0.96f));
            var title = Label(root.transform, "ЗАЗЕРКАЛЬЕ", 64, TextAnchor.UpperCenter);
            var rt = title.rectTransform;
            rt.anchorMin = new Vector2(0.1f, 0.62f);
            rt.anchorMax = new Vector2(0.9f, 0.88f);
            title.color = VisualPalette.UiAccent;

            var sub = Label(root.transform, "Сумеречная роща · сессия 2–5 минут", 28, TextAnchor.UpperCenter);
            sub.rectTransform.anchorMin = new Vector2(0.15f, 0.52f);
            sub.rectTransform.anchorMax = new Vector2(0.85f, 0.62f);
            sub.color = VisualPalette.UiText * 0.85f;

            var blurb = Label(root.transform,
                "Третье лицо. Кокалка. Рывок. Короткие заходы без открытого мира — как в GDD.",
                22, TextAnchor.MiddleCenter);
            blurb.rectTransform.anchorMin = new Vector2(0.18f, 0.40f);
            blurb.rectTransform.anchorMax = new Vector2(0.82f, 0.52f);
            blurb.color = VisualPalette.UiText * 0.7f;

            Button(root.transform, "В рощу", new Vector2(0.35f, 0.26f), new Vector2(0.65f, 0.34f), () => OnStartMatch?.Invoke());
            Button(root.transform, "Бестиарий", new Vector2(0.35f, 0.16f), new Vector2(0.65f, 0.24f), ShowBestiary);

            var hint = Label(root.transform, "WASD · Пробел — кок · Shift — рывок", 18, TextAnchor.LowerCenter);
            hint.rectTransform.anchorMin = new Vector2(0.2f, 0.04f);
            hint.rectTransform.anchorMax = new Vector2(0.8f, 0.12f);
            hint.color = VisualPalette.UiText * 0.5f;
            return root;
        }

        GameObject BuildHud()
        {
            var root = Panel("Hud", new Color(0, 0, 0, 0));
            root.GetComponent<Image>().raycastTarget = false;

            _hudGoal = Label(root.transform, "", 22, TextAnchor.UpperCenter);
            _hudGoal.rectTransform.anchorMin = new Vector2(0.15f, 0.90f);
            _hudGoal.rectTransform.anchorMax = new Vector2(0.85f, 0.98f);
            _hudGoal.color = VisualPalette.UiText;

            _hudStats = Label(root.transform, "", 26, TextAnchor.UpperLeft);
            _hudStats.rectTransform.anchorMin = new Vector2(0.03f, 0.72f);
            _hudStats.rectTransform.anchorMax = new Vector2(0.45f, 0.90f);
            _hudStats.alignment = TextAnchor.UpperLeft;
            _hudStats.color = VisualPalette.UiText;

            _toast = Label(root.transform, "", 24, TextAnchor.UpperCenter);
            _toast.rectTransform.anchorMin = new Vector2(0.2f, 0.78f);
            _toast.rectTransform.anchorMax = new Vector2(0.8f, 0.86f);
            _toast.color = VisualPalette.UiAccent;

            var vignette = new GameObject("Vignette", typeof(RectTransform), typeof(Image));
            vignette.transform.SetParent(root.transform, false);
            var vrt = vignette.GetComponent<RectTransform>();
            vrt.anchorMin = Vector2.zero;
            vrt.anchorMax = Vector2.one;
            vrt.offsetMin = Vector2.zero;
            vrt.offsetMax = Vector2.zero;
            var img = vignette.GetComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.18f);
            img.raycastTarget = false;
            vignette.transform.SetAsFirstSibling();
            return root;
        }

        GameObject BuildResults()
        {
            var root = Panel("Results", VisualPalette.UiBg);
            var title = Label(root.transform, "Итог захода", 48, TextAnchor.UpperCenter);
            title.rectTransform.anchorMin = new Vector2(0.2f, 0.78f);
            title.rectTransform.anchorMax = new Vector2(0.8f, 0.92f);
            title.color = VisualPalette.UiAccent;

            _resultsBody = Label(root.transform, "", 28, TextAnchor.UpperLeft);
            _resultsBody.rectTransform.anchorMin = new Vector2(0.25f, 0.35f);
            _resultsBody.rectTransform.anchorMax = new Vector2(0.75f, 0.76f);
            _resultsBody.alignment = TextAnchor.UpperLeft;

            Button(root.transform, "×2 награда (реклама)", new Vector2(0.30f, 0.22f), new Vector2(0.70f, 0.30f), () =>
            {
                _ = DoubleRewardAsync();
            });
            Button(root.transform, "Ещё заход", new Vector2(0.30f, 0.12f), new Vector2(0.70f, 0.20f), () =>
            {
                _ = ReplayAsync();
            });
            Button(root.transform, "В меню", new Vector2(0.30f, 0.04f), new Vector2(0.70f, 0.11f), () =>
            {
                OnBackToMenu?.Invoke();
                ShowMenu();
            });
            return root;
        }

        GameObject BuildBestiary()
        {
            var root = Panel("Bestiary", VisualPalette.UiBg);
            var title = Label(root.transform, "Бестиарий", 48, TextAnchor.UpperCenter);
            title.rectTransform.anchorMin = new Vector2(0.2f, 0.86f);
            title.rectTransform.anchorMax = new Vector2(0.8f, 0.96f);
            title.color = VisualPalette.UiAccent;

            var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(Image), typeof(ScrollRect));
            scrollGo.transform.SetParent(root.transform, false);
            var srt = scrollGo.GetComponent<RectTransform>();
            srt.anchorMin = new Vector2(0.18f, 0.16f);
            srt.anchorMax = new Vector2(0.82f, 0.84f);
            srt.offsetMin = Vector2.zero;
            srt.offsetMax = Vector2.zero;
            scrollGo.GetComponent<Image>().color = new Color(0.05f, 0.07f, 0.08f, 0.6f);

            var content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(scrollGo.transform, false);
            var crt = content.GetComponent<RectTransform>();
            crt.anchorMin = new Vector2(0f, 1f);
            crt.anchorMax = new Vector2(1f, 1f);
            crt.pivot = new Vector2(0.5f, 1f);
            crt.sizeDelta = new Vector2(0f, 1800f);

            _bestiaryBody = Label(content.transform, "", 22, TextAnchor.UpperLeft);
            _bestiaryBody.rectTransform.anchorMin = new Vector2(0.04f, 0f);
            _bestiaryBody.rectTransform.anchorMax = new Vector2(0.96f, 1f);
            _bestiaryBody.alignment = TextAnchor.UpperLeft;
            _bestiaryBody.horizontalOverflow = HorizontalWrapMode.Wrap;
            _bestiaryBody.verticalOverflow = VerticalWrapMode.Overflow;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.content = crt;
            scroll.horizontal = false;
            scroll.viewport = srt;

            Button(root.transform, "Назад", new Vector2(0.35f, 0.04f), new Vector2(0.65f, 0.12f), ShowMenu);
            return root;
        }

        GameObject Panel(string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_canvas.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = color;
            return go;
        }

        Text Label(Transform parent, string text, int size, TextAnchor anchor)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = _font;
            t.text = text;
            t.fontSize = size;
            t.alignment = anchor;
            t.color = VisualPalette.UiText;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            var rt = t.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return t;
        }

        async System.Threading.Tasks.Task DoubleRewardAsync()
        {
            bool ok = await YandexGamesSdk.ShowRewarded();
            if (!ok) return;
            var save = SaveService.Load();
            save.Kukichi += _lastResult.Kukichi;
            SaveService.Write(save);
            Toast("Награда удвоена");
        }

        async System.Threading.Tasks.Task ReplayAsync()
        {
            await YandexGamesSdk.ShowInterstitial();
            OnStartMatch?.Invoke();
        }

        void Button(Transform parent, string text, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject("Btn_" + text, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = new Color(0.16f, 0.20f, 0.22f, 0.95f);
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);
            var label = Label(go.transform, text, 24, TextAnchor.MiddleCenter);
            label.color = VisualPalette.UiText;
        }
    }
}
