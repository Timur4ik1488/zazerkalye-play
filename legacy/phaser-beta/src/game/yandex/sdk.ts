export type YandexSdk = {
  isMock: boolean;
  ready: () => void;
  showInterstitial: () => Promise<void>;
  showRewarded: () => Promise<boolean>;
};

declare global {
  interface Window {
    YaGames?: {
      init: () => Promise<{
        features?: { LoadingAPI?: { ready: () => void } };
        adv?: {
          showFullscreenAdv: (opts: {
            callbacks?: { onClose?: (wasShown: boolean) => void; onError?: (e: unknown) => void };
          }) => void;
          showRewardedVideo: (opts: {
            callbacks?: {
              onRewarded?: () => void;
              onClose?: (wasShown: boolean) => void;
              onError?: (e: unknown) => void;
            };
          }) => void;
        };
      }>;
    };
  }
}

let cached: YandexSdk | null = null;

export async function getYandexSdk(): Promise<YandexSdk> {
  if (cached) return cached;

  if (!window.YaGames?.init) {
    cached = {
      isMock: true,
      ready: () => console.info('[YaGames mock] ready'),
      showInterstitial: async () => console.info('[YaGames mock] interstitial'),
      showRewarded: async () => true,
    };
    return cached;
  }

  const ysdk = await window.YaGames.init();
  cached = {
    isMock: false,
    ready: () => ysdk.features?.LoadingAPI?.ready?.(),
    showInterstitial: () =>
      new Promise((resolve) => {
        if (!ysdk.adv?.showFullscreenAdv) return resolve();
        ysdk.adv.showFullscreenAdv({
          callbacks: { onClose: () => resolve(), onError: () => resolve() },
        });
      }),
    showRewarded: () =>
      new Promise((resolve) => {
        let rewarded = false;
        if (!ysdk.adv?.showRewardedVideo) return resolve(false);
        ysdk.adv.showRewardedVideo({
          callbacks: {
            onRewarded: () => {
              rewarded = true;
            },
            onClose: () => resolve(rewarded),
            onError: () => resolve(false),
          },
        });
      }),
  };
  return cached;
}
