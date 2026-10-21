using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
namespace TimeGate.Services
{
    /// Honest stubs: nothing here pretends to be connected. Real SDK wiring needs your keys/files.
    public interface IAdService { bool IsConfigured { get; } string Status { get; } }
    public interface ICloudSync { bool IsConfigured { get; } string Status { get; } }
    public interface IAiGuide
    {
        bool IsConfigured { get; } string Status { get; }
        IEnumerator Ask(string characterId, string question, string lang, Action<string> ok, Action<string> fail);
    }

    public class NullAdService : IAdService { public bool IsConfigured => false; public string Status => "Start.io: needs App ID + SDK package (see docs/SERVICES.md)"; }
    public class NullCloudSync : ICloudSync { public bool IsConfigured => false; public string Status => "Firebase/Supabase: needs config files/keys (see docs/SERVICES.md)"; }
    public class NullAiGuide : IAiGuide
    {
        public bool IsConfigured => false; public string Status => "AI guide: needs secure proxy URL (no API keys in APK)";
        public IEnumerator Ask(string c, string q, string l, Action<string> ok, Action<string> fail) { fail("not configured"); yield break; }
    }

    [Serializable] public class RuntimeConfig { public string aiProxyUrl; }
    [Serializable] class AiReq { public string characterId; public string question; public string lang; }
    [Serializable] class AiRes { public string answer; }

    /// Talks to YOUR server (contract in docs/AI_PROXY.md). The LLM key lives on the server only.
    public class AiProxyGuide : IAiGuide
    {
        readonly string url; float last = -100f;
        public AiProxyGuide(string u) { url = u; }
        public bool IsConfigured => true; public string Status => "AI guide: proxy URL set (untested)";
        public IEnumerator Ask(string characterId, string question, string lang, Action<string> ok, Action<string> fail)
        {
            if (Time.realtimeSinceStartup - last < 3f) { fail("rate limit"); yield break; }
            last = Time.realtimeSinceStartup;
            if (question.Length > 300) question = question.Substring(0, 300);
            var body = JsonUtility.ToJson(new AiReq { characterId = characterId, question = question, lang = lang });
            using (var rq = new UnityWebRequest(url, "POST"))
            {
                rq.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)); rq.downloadHandler = new DownloadHandlerBuffer();
                rq.SetRequestHeader("Content-Type", "application/json"); rq.timeout = 15;
                yield return rq.SendWebRequest();
                if (rq.result != UnityWebRequest.Result.Success) { fail(rq.error); yield break; }
                AiRes r = null; try { r = JsonUtility.FromJson<AiRes>(rq.downloadHandler.text); } catch { }
                if (r == null || string.IsNullOrEmpty(r.answer)) fail("empty response");
                else ok(r.answer.Length > 600 ? r.answer.Substring(0, 600) : r.answer);
            }
        }
    }

    public static class ServiceHub
    {
        public static IAdService Ads = new NullAdService();
        public static ICloudSync Cloud = new NullCloudSync();
        public static IAiGuide Ai = new NullAiGuide();
        public static void LoadRuntimeConfig()
        {
            var ta = Resources.Load<TextAsset>("runtime_config"); if (ta == null) return;
            try { var c = JsonUtility.FromJson<RuntimeConfig>(ta.text); if (c != null && !string.IsNullOrEmpty(c.aiProxyUrl) && c.aiProxyUrl.StartsWith("https://")) Ai = new AiProxyGuide(c.aiProxyUrl); }
            catch (Exception e) { Debug.LogWarning("[TimeGate] runtime_config invalid: " + e.Message); }
        }
    }
}
