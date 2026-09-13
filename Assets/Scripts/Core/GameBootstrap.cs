using UnityEngine;
using UnityEngine.Rendering.Universal;
using Zazerkalye.Player;
using Zazerkalye.UI;
using Zazerkalye.Visual;
using Zazerkalye.World;
using Zazerkalye.Yandex;

namespace Zazerkalye.Core
{
    /// <summary>
    /// Code-first entry: builds lighting, grove, player, match loop and UI at runtime.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        MatchController _match;
        GameUI _ui;
        GameObject _matchRoot;
        Light _keyLight;
        GroveBuilder _grove;
        SecretHolder _secrets;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoBoot()
        {
            if (FindFirstObjectByType<GameBootstrap>() != null) return;
            var go = new GameObject("GameBootstrap");
            go.AddComponent<GameBootstrap>();
        }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            GameplayInput.Clear();

            try { SetupCameraAndLighting(); }
            catch (System.Exception e) { Debug.LogWarning("[Зазеркалье] Lighting skipped: " + e.Message); }

            _matchRoot = new GameObject("MatchRoot");
            _matchRoot.transform.SetParent(transform, false);
            _matchRoot.SetActive(false);

            BuildWorld(_matchRoot.transform);
            var player = BuildPlayer(_matchRoot.transform);
            var camComp = Camera.main != null ? Camera.main.GetComponent<ThirdPersonCamera>() : null;
            if (camComp != null) camComp.Target = player.transform;

            _match = _matchRoot.AddComponent<MatchController>();
            _match.Player = player;
            _match.Cam = camComp;
            _match.WorldRoot = _matchRoot.transform;
            _match.Grove = _grove;
            _match.KeyLight = _keyLight;

            var uiGo = new GameObject("GameUI");
            uiGo.transform.SetParent(transform, false);
            _ui = uiGo.AddComponent<GameUI>();
            _ui.Build(_match);
            _ui.OnStartMatch = StartMatch;
            _ui.OnBackToMenu = () =>
            {
                _matchRoot.SetActive(false);
                GameplayInput.Clear();
                Time.timeScale = 1f;
            };
            _match.OnToast = msg => _ui.Toast(msg);
            _match.OnFinished = result =>
            {
                Time.timeScale = 1f;
                _matchRoot.SetActive(false);
                GameplayInput.Clear();
                _ui.ShowResults(result);
            };
            _match.OnHudDirty = () => _ui.RefreshHud();

            YandexGamesSdk.Ready();
        }

        void SetupCameraAndLighting()
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                cam.tag = "MainCamera";
                camGo.AddComponent<AudioListener>();
            }

            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = VisualPalette.Fog;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 120f;
            cam.fieldOfView = 55f;

            try
            {
                var urpCam = cam.GetComponent<UniversalAdditionalCameraData>();
                if (urpCam == null) urpCam = cam.gameObject.AddComponent<UniversalAdditionalCameraData>();
                urpCam.renderPostProcessing = true;
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("URP camera data skipped (assign URP via Зазеркалье → Configure URP): " + e.Message);
            }

            if (cam.GetComponent<ThirdPersonCamera>() == null)
                cam.gameObject.AddComponent<ThirdPersonCamera>();

            _keyLight = CreateDirectionalLight("KeyLight", VisualPalette.KeyLight, 1.15f, new Vector3(38f, -35f, 0f), LightShadows.Soft);
            CreateDirectionalLight("FillLight", VisualPalette.FillLight, 0.35f, new Vector3(15f, 140f, 0f), LightShadows.None);
        }

        Light CreateDirectionalLight(string name, Color color, float intensity, Vector3 euler, LightShadows shadows)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.rotation = Quaternion.Euler(euler);
            var light = go.GetComponent<Light>();
            if (light == null) light = go.AddComponent<Light>();
            if (light == null)
            {
                Debug.LogWarning("[Зазеркалье] Light component missing on " + name);
                return null;
            }
            light.type = LightType.Directional;
            light.color = color;
            light.intensity = intensity;
            light.shadows = shadows;
            try
            {
                if (go.GetComponent<UniversalAdditionalLightData>() == null)
                    go.AddComponent<UniversalAdditionalLightData>();
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("[Зазеркалье] URP light data skipped: " + e.Message);
            }
            return light;
        }

        void BuildWorld(Transform root)
        {
            _grove = root.gameObject.AddComponent<GroveBuilder>();
            _grove.Build(root);
            _grove.BuildSwamps(root);

            var idolGo = new GameObject("Istukanus");
            idolGo.transform.SetParent(root, false);
            idolGo.transform.position = new Vector3(-18f, 0f, 16f);
            var idol = idolGo.AddComponent<SecretInteractable>();
            idol.InitIdol();

            var wellGo = new GameObject("SpectralWell");
            wellGo.transform.SetParent(root, false);
            wellGo.transform.position = new Vector3(20f, 0f, -14f);
            var well = wellGo.AddComponent<SecretInteractable>();
            well.InitWell();
            var wellTree = MeshFactory.Cylinder("WellTree", new Vector3(0.4f, 2.2f, 0.4f), VisualPalette.Trunk, wellGo.transform);
            wellTree.transform.localPosition = new Vector3(-1.6f, 2.2f, 0.4f);
            var wellCanopy = MeshFactory.Sphere("WellCanopy", Vector3.one * 2.2f, VisualPalette.Canopy, wellGo.transform);
            wellCanopy.transform.localPosition = new Vector3(-1.6f, 4.4f, 0.4f);

            var polenychGo = new GameObject("Polenych");
            polenychGo.transform.SetParent(root, false);
            polenychGo.transform.position = new Vector3(2.4f, 0f, 3.2f);
            var polenych = polenychGo.AddComponent<GroveNpc>();
            polenych.Init("polenych",
                "Поленыч преклонил колено. ПВЗ открыт. Три следа пацаноида — и равновесие вернётся.",
                NpcLook.Tree, kneeling: true);

            var akakyGo = new GameObject("Akaky");
            akakyGo.transform.SetParent(root, false);
            akakyGo.transform.position = new Vector3(9f, 0f, 11f);
            var akaky = akakyGo.AddComponent<GroveNpc>();
            akaky.Init("akaky",
                "Акакий Куролесов: «Днём кокай, ночью беги от жвачников. Ночь можно переждать у меня.»",
                NpcLook.Humanoid, repeatable: true);

            var kazimirGo = new GameObject("Kazimir");
            kazimirGo.transform.SetParent(root, false);
            kazimirGo.transform.position = new Vector3(-14f, 0f, 6f);
            var kazimir = kazimirGo.AddComponent<GroveNpc>();
            kazimir.Init("kazimir",
                "Казимир унёс ложку пацаноидов. Их двое — без ложек равновесие ещё злее.",
                NpcLook.Wolf);

            var mihailGo = new GameObject("Mihail");
            mihailGo.transform.SetParent(root, false);
            mihailGo.transform.position = new Vector3(16f, 0f, -10f);
            var mihail = mihailGo.AddComponent<GroveNpc>();
            mihail.Init("mihail",
                "Михаил ищет скорлупки. Не стой между ним и бобылем.",
                NpcLook.Shell);

            var pedalGo = new GameObject("PedalBoy");
            pedalGo.transform.SetParent(root, false);
            pedalGo.transform.position = new Vector3(-6f, 0f, 12f);
            var pedal = pedalGo.AddComponent<GroveNpc>();
            pedal.Init("pedal",
                "Мальчик-педаль нажал — и ноги сами побежали.",
                NpcLook.Pedal);

            var kolenychGo = new GameObject("Kolenych");
            kolenychGo.transform.SetParent(root, false);
            kolenychGo.transform.position = new Vector3(-8f, 0f, -16f);
            var kolenych = kolenychGo.AddComponent<GroveNpc>();
            kolenych.Init("kolenych",
                "Коленыч преклонил полено! Рывок зигзагом!",
                NpcLook.Bone, hazard: true);

            var scripachGo = new GameObject("Scripach");
            scripachGo.transform.SetParent(root, false);
            scripachGo.transform.position = new Vector3(14f, 0f, 18f);
            var scripach = scripachGo.AddComponent<GroveNpc>();
            scripach.Init("scripach",
                "Болотный Скрипач ведёт в сторону от колодца. Маска кивает не туда.",
                NpcLook.Mask, nightOnly: true);

            _secrets = root.gameObject.AddComponent<SecretHolder>();
            _secrets.Mirror = idol;
            _secrets.Well = well;
            _secrets.Polenych = polenych;
            _secrets.Akaky = akaky;
            _secrets.Kazimir = kazimir;
            _secrets.Mihail = mihail;
            _secrets.Pedal = pedal;
            _secrets.Kolenych = kolenych;
            _secrets.Scripach = scripach;
        }

        PlayerController BuildPlayer(Transform root)
        {
            var go = new GameObject("Player");
            go.transform.SetParent(root, false);
            go.transform.position = Vector3.zero;
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.9f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.95f, 0f);
            cc.skinWidth = 0.08f;
            cc.minMoveDistance = 0f;
            return go.AddComponent<PlayerController>();
        }

        void StartMatch()
        {
            Time.timeScale = 1f;
            GameplayInput.Clear();
            _matchRoot.SetActive(true);
            _match.PlaceWorld(_secrets.Mirror, _secrets.Well, new[]
            {
                _secrets.Polenych, _secrets.Akaky, _secrets.Kazimir, _secrets.Mihail,
                _secrets.Pedal, _secrets.Kolenych, _secrets.Scripach
            });
            _match.Player.ResetForMatch();
            _match.Player.Warp(Vector3.zero);
            _match.Cam?.SnapToTarget();
            _match.Begin();
            _ui.ShowHud();
        }

        class SecretHolder : MonoBehaviour
        {
            public SecretInteractable Mirror;
            public SecretInteractable Well;
            public GroveNpc Polenych;
            public GroveNpc Akaky;
            public GroveNpc Kazimir;
            public GroveNpc Mihail;
            public GroveNpc Pedal;
            public GroveNpc Kolenych;
            public GroveNpc Scripach;
        }
    }
}
