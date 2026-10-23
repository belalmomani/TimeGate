using TimeGate.Data;
using TimeGate.Services;
namespace TimeGate.Core
{
    /// SDK-independent rules for when an ad MAY appear. Ads are never shown during flight or dialogue,
    /// only back at the gate, only with consent, and rarely.
    public static class AdPolicy
    {
        public const int MinTripsBetweenAds = 2; public const float MinSecondsBetweenAds = 240f;
        static float lastShown = -9999f;

        public static bool Allowed(SaveData d, bool atGate, float now) =>
            atGate && ServiceHub.Ads.IsConfigured && d.adsConsent == 1 && d.tripsSinceAd >= MinTripsBetweenAds && now - lastShown >= MinSecondsBetweenAds;

        public static void TryShow(SaveSystem save, bool atGate, float now)
        {
            if (!Allowed(save.Data, atGate, now)) return;
            ServiceHub.Ads.ShowInterstitial(shown => { if (shown) { lastShown = now; save.Data.tripsSinceAd = 0; save.Save(); } });
        }
    }
}
