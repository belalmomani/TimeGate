using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
namespace TimeGate.Services
{
    public static class Http
    {
        /// cb(statusCode, bodyText, transportError). Only https URLs are allowed.
        public static IEnumerator Send(string method, string url, string body, Dictionary<string, string> headers, Action<long, string, string> cb)
        {
            if (!url.StartsWith("https://")) { cb(0, "", "https required"); yield break; }
            using (var rq = new UnityWebRequest(url, method))
            {
                if (body != null) { rq.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)); }
                rq.downloadHandler = new DownloadHandlerBuffer(); rq.timeout = 20;
                if (headers != null) foreach (var kv in headers) rq.SetRequestHeader(kv.Key, kv.Value);
                yield return rq.SendWebRequest();
                bool transport = rq.result == UnityWebRequest.Result.ConnectionError || rq.result == UnityWebRequest.Result.DataProcessingError;
                cb(rq.responseCode, rq.downloadHandler != null ? rq.downloadHandler.text : "", transport ? rq.error : null);
            }
        }
    }
}
