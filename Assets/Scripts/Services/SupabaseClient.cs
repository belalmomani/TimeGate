using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TimeGate.Services
{
    [Serializable] class SbPack { public string id; public string pack_version; public string status; }
    [Serializable] class SbPackList { public SbPack[] items; }

    /// Read-only content access with the public ANON key (RLS only exposes published content).
    public class SupabaseClient
    {
        readonly string url, anon;
        public SupabaseClient(string u, string a) { url = u.TrimEnd('/'); anon = a; }
        public string Status => "Supabase: configured (anon key, read-only)";
        public IEnumerator Get(string pathAndQuery, Action<string> ok, Action<string> fail)
        {
            var h = new Dictionary<string, string> { ["apikey"] = anon, ["Authorization"] = "Bearer " + anon };
            long code = 0; string res = null, terr = null;
            yield return Http.Send("GET", url + "/rest/v1/" + pathAndQuery, null, h, (c, b, e) => { code = c; res = b; terr = e; });
            if (code == 200) ok(res); else fail(terr ?? ("HTTP " + code));
        }
        /// Returns "id vX" lines for published packs, so the UI can show available content updates.
        public IEnumerator PublishedPacks(Action<List<string>> ok, Action<string> fail)
        {
            string raw = null, err = null;
            yield return Get("packs?select=id,pack_version,status&published=eq.true", r => raw = r, e => err = e);
            if (err != null) { fail(err); yield break; }
            try { var l = JsonUtility.FromJson<SbPackList>("{\"items\":" + raw + "}"); var o = new List<string>(); foreach (var p in l.items) o.Add(p.id + " v" + p.pack_version + " (" + p.status + ")"); ok(o); }
            catch (Exception ex) { fail("bad response: " + ex.Message); }
        }
    }
}
