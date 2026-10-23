using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;
namespace TimeGate.Travel
{
    /// Resumable, hash-verified download for future content packs (current Petra pack is bundled, so unused yet).
    /// Limits: Unity has no portable free-disk API, so low storage is detected as a write failure, not predicted.
    public static class PackDownloader
    {
        public static string PackPath(string packId) => Path.Combine(Application.persistentDataPath, "packs", packId + ".pack");

        public static IEnumerator Download(string packId, string url, string sha256Hex, Action<float> progress, Action<string> done, Action<string> fail)
        {
            if (!url.StartsWith("https://")) { fail("https required"); yield break; }
            string final = PackPath(packId), part = final + ".part";
            Directory.CreateDirectory(Path.GetDirectoryName(final));
            if (File.Exists(final) && Verify(final, sha256Hex)) { done(final); yield break; }
            long have = File.Exists(part) ? new FileInfo(part).Length : 0;
            using (var rq = new UnityWebRequest(url, "GET"))
            {
                if (have > 0) rq.SetRequestHeader("Range", "bytes=" + have + "-");
                rq.downloadHandler = new DownloadHandlerFile(part, have > 0) { removeFileOnAbort = false };
                rq.timeout = 30; var op = rq.SendWebRequest();
                while (!op.isDone) { progress?.Invoke(rq.downloadProgress); yield return null; }
                if (rq.result != UnityWebRequest.Result.Success)
                {
                    // 416 = range not satisfiable: part file is stale; restart next attempt
                    if (rq.responseCode == 416) { try { File.Delete(part); } catch { } }
                    fail(rq.error + " (resume possible on retry)"); yield break;
                }
                if (have > 0 && rq.responseCode == 200) { fail("server ignored range; retry to restart"); try { File.Delete(part); } catch { } yield break; }
            }
            if (!Verify(part, sha256Hex)) { try { File.Delete(part); } catch { } fail("hash mismatch (file deleted)"); yield break; }
            try { if (File.Exists(final)) File.Delete(final); File.Move(part, final); } catch (Exception e) { fail("storage error: " + e.Message); yield break; }
            progress?.Invoke(1f); done(final);
        }

        public static bool Verify(string path, string sha256Hex)
        {
            try
            {
                using (var fs = File.OpenRead(path)) using (var sha = SHA256.Create())
                {
                    var h = BitConverter.ToString(sha.ComputeHash(fs)).Replace("-", "").ToLowerInvariant();
                    return h == (sha256Hex ?? "").ToLowerInvariant();
                }
            }
            catch { return false; }
        }
    }
}
