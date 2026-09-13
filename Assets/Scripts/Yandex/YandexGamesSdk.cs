using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Zazerkalye.Yandex
{
    /// <summary>
    /// Thin YaGames bridge. In Editor / Desktop uses mock. WebGL uses .jslib when present.
    /// </summary>
    public static class YandexGamesSdk
    {
        public static bool IsMock { get; private set; } = true;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void YaGames_Ready();
        [DllImport("__Internal")] static extern void YaGames_ShowFullscreen();
        [DllImport("__Internal")] static extern void YaGames_ShowRewarded();
#endif

        public static void Ready()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try { YaGames_Ready(); IsMock = false; }
            catch (Exception e) { Debug.LogWarning("[YaGames] ready failed: " + e.Message); }
#else
            Debug.Log("[YaGames mock] ready");
#endif
        }

        public static Task ShowInterstitial()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try { YaGames_ShowFullscreen(); IsMock = false; }
            catch (Exception e) { Debug.LogWarning("[YaGames] interstitial: " + e.Message); }
            return Task.CompletedTask;
#else
            Debug.Log("[YaGames mock] interstitial");
            return Task.CompletedTask;
#endif
        }

        public static Task<bool> ShowRewarded()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            try { YaGames_ShowRewarded(); IsMock = false; return Task.FromResult(true); }
            catch (Exception e) { Debug.LogWarning("[YaGames] rewarded: " + e.Message); return Task.FromResult(false); }
#else
            Debug.Log("[YaGames mock] rewarded → granted");
            return Task.FromResult(true);
#endif
        }
    }
}
