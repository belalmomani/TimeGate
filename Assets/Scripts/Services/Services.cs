using UnityEngine;
namespace TimeGate.Services
{
    /// Honest stubs: nothing here pretends to be connected. Real SDK wiring needs your keys/files.
    public interface IAdService { bool IsConfigured { get; } string Status { get; } }
    public interface ICloudSync { bool IsConfigured { get; } string Status { get; } }
    public interface IAiGuide { bool IsConfigured { get; } string Status { get; } }

    public class NullAdService : IAdService { public bool IsConfigured => false; public string Status => "Start.io: needs App ID + SDK package (see docs/SERVICES.md)"; }
    public class NullCloudSync : ICloudSync { public bool IsConfigured => false; public string Status => "Firebase/Supabase: needs config files/keys (see docs/SERVICES.md)"; }
    public class NullAiGuide : IAiGuide { public bool IsConfigured => false; public string Status => "AI guide: needs secure proxy URL (no API keys in APK)"; }

    public static class ServiceHub
    {
        public static IAdService Ads = new NullAdService();
        public static ICloudSync Cloud = new NullCloudSync();
        public static IAiGuide Ai = new NullAiGuide();
    }
}
