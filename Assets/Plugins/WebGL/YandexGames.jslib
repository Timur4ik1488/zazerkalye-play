mergeInto(LibraryManager.library, {
  YaGames_Ready: function () {
    if (typeof YaGames === 'undefined') {
      console.info('[YaGames] SDK missing — stub ready');
      return;
    }
    YaGames.init().then(function (ysdk) {
      window.__ysdk = ysdk;
      if (ysdk.features && ysdk.features.LoadingAPI) ysdk.features.LoadingAPI.ready();
    }).catch(function (e) { console.warn(e); });
  },
  YaGames_ShowFullscreen: function () {
    var ysdk = window.__ysdk;
    if (!ysdk || !ysdk.adv) { console.info('[YaGames] stub interstitial'); return; }
    ysdk.adv.showFullscreenAdv({ callbacks: {} });
  },
  YaGames_ShowRewarded: function () {
    var ysdk = window.__ysdk;
    if (!ysdk || !ysdk.adv) { console.info('[YaGames] stub rewarded'); return; }
    ysdk.adv.showRewardedVideo({ callbacks: {} });
  }
});
