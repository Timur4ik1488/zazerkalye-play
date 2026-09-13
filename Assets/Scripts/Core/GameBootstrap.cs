using UnityEngine;
using UnityEngine.Rendering.Universal;
using Zazerkalye.Core;
using Zazerkalye.Data;
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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoBoot()
        {
            if (FindObjectOfType<GameBootstrap>() != null) return;
            var go = new GameObject("GameBootstrap");
            go.AddComponent<GameBootstrap>();
        }

        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;

            SetupCameraAndLighting();
            _matchRoot = new GameObject("MatchRoot");
            _matchRoot.transform.SetParent(transform, false);
            _matchRoot.SetActive(false);

            BuildWorld(_matchRoot.transform);
            var player = BuildPlayer(_matchRoot.transform);
            var cam = Camera.main.gameObject.GetComponent<ThirdPersonCamera>();
            cam.Target = player.transform;

            _match = _matchRoot.AddComponent<MatchController>();
            _match.Player = player;
            _match.Cam = cam;
            _match.WorldRoot = _matchRoot.transform;

            var uiGo = new GameObject("GameUI");
            uiGo.transform.SetParent(transform, false);
            _ui = uiGo.AddComponent<GameUI>();
            _ui.Build(_match);
            _ui.OnStartMatch = StartMatch;
            _ui.OnBackToMenu = () =>
            {
                _matchRoot.SetActive(false);
                Time.timeScale = 1f;
            };
            _match.OnToast = msg => _ui.Toast(msg);
            _match.OnFinished = result =>
            {
                Time.timeScale = 1f;
                _matchRoot.SetActive(false);
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

            // Key light
            var keyGo = GameObject.Find("KeyLight") ?? new GameObject("KeyLight");
            _keyLight = keyGo.GetComponent<Light>() ?? keyGo.AddComponent<Light>();
            _keyLight.type = LightType.Directional;
            _keyLight.color = VisualPalette.KeyLight;
            _keyLight.intensity = 1.15f;
            _keyLight.shadows = LightShadows.Soft;
            keyGo.transform.rotation = Quaternion.Euler(38f, -35f, 0f);

            var fillGo = GameObject.Find("FillLight") ?? new GameObject("FillLight");
            var fill = fillGo.GetComponent<Light>() ?? fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = VisualPalette.FillLight;
            fill.intensity = 0.35f;
            fill.shadows = LightShadows.None;
            fillGo.transform.rotation = Quaternion.Euler(15f, 140f, 0f);
        }

        void BuildWorld(Transform root)
        {
            var builder = root.gameObject.AddComponent<GroveBuilder>();
            builder.Build(root);
            builder.BuildSwamps(root,
                _ => _match?.NotifySwamp(true),
                _ => _match?.NotifySwamp(false));

            // Secrets far apart
            var mirrorGo = new GameObject("IstukanusMirror");
            mirrorGo.transform.SetParent(root, false);
            mirrorGo.transform.position = new Vector3(-18f, 0f, 16f);
            var mirror = mirrorGo.AddComponent<SecretInteractable>();
            mirror.InitMirror();

            var wellGo = new GameObject("SpectralWell");
            wellGo.transform.SetParent(root, false);
            wellGo.transform.position = new Vector3(20f, 0f, -14f);
            var well = wellGo.AddComponent<SecretInteractable>();
            well.InitWell();

            // Stash for match begin
            var holder = root.gameObject.AddComponent<SecretHolder>();
            holder.Mirror = mirror;
            holder.Well = well;
        }

        PlayerController BuildPlayer(Transform root)
        {
            var go = new GameObject("Player");
            go.transform.SetParent(root, false);
            go.transform.position = new Vector3(0f, 0f, 0f);
            var cc = go.AddComponent<CharacterController>();
            cc.height = 1.9f;
            cc.radius = 0.35f;
            cc.center = new Vector3(0f, 0.95f, 0f);
            cc.skinWidth = 0.08f;
            return go.AddComponent<PlayerController>();
        }

        void StartMatch()
        {
            _matchRoot.SetActive(true);
            var holder = _matchRoot.GetComponent<SecretHolder>();
            _match.PlaceSecrets(holder.Mirror, holder.Well);
            // Reset player
            _match.Player.transform.position = Vector3.zero;
            _match.Begin();
            _ui.ShowHud();
            Time.timeScale = 1f;
        }

        class SecretHolder : MonoBehaviour
        {
            public SecretInteractable Mirror;
            public SecretInteractable Well;
        }
    }
}
