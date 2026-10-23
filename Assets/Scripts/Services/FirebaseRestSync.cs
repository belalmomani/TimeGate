using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TimeGate.Data;
namespace TimeGate.Services
{
    [Serializable] class FbAuthReq { public string email; public string password; public bool returnSecureToken = true; }
    [Serializable] class FbAuthRes { public string idToken; public string refreshToken; public string localId; public string email; public string expiresIn; }
    [Serializable] class FbRefreshRes { public string id_token; public string refresh_token; public string user_id; public string expires_in; }
    [Serializable] class FbStr { public string stringValue; }
    [Serializable] class FbFields { public FbStr progress; public FbStr updatedUtc; }
    [Serializable] class FbDoc { public FbFields fields; }
    [Serializable] class FbIdTokenReq { public string idToken; }

    /// Firebase Auth + Firestore through the public REST APIs (no SDK, no google-services.json needed).
    /// Needs only the Web API key + project id (public values). Security is enforced by firebase/firestore.rules.
    public class FirebaseRestSync : ICloudSync
    {
        readonly string apiKey, projectId; string idToken, refreshToken, uid, email; DateTime expiresAt;
        public FirebaseRestSync(string key, string project)
        {
            apiKey = key; projectId = project;
            refreshToken = PlayerPrefs.GetString("tg_fb_refresh", ""); uid = PlayerPrefs.GetString("tg_fb_uid", ""); email = PlayerPrefs.GetString("tg_fb_email", "");
        }
        public bool IsConfigured => true;
        public bool SignedIn => !string.IsNullOrEmpty(refreshToken);
        public string Email => email;
        public string Status => SignedIn ? "Firebase (REST): signed in as " + email : "Firebase (REST): configured, signed out";

        static string AuthUrl(string op, string key) => "https://identitytoolkit.googleapis.com/v1/accounts:" + op + "?key=" + key;
        static Dictionary<string, string> Json() => new Dictionary<string, string> { ["Content-Type"] = "application/json" };
        Dictionary<string, string> Bearer() => new Dictionary<string, string> { ["Content-Type"] = "application/json", ["Authorization"] = "Bearer " + idToken };
        string DocUrl() => "https://firestore.googleapis.com/v1/projects/" + projectId + "/databases/(default)/documents/users/" + uid;

        void Store(string id, string refresh, string user, string mail, string expiresIn)
        {
            idToken = id; refreshToken = refresh; uid = user; if (!string.IsNullOrEmpty(mail)) email = mail;
            int secs; if (!int.TryParse(expiresIn, out secs)) secs = 3000; expiresAt = DateTime.UtcNow.AddSeconds(secs - 60);
            PlayerPrefs.SetString("tg_fb_refresh", refreshToken ?? ""); PlayerPrefs.SetString("tg_fb_uid", uid ?? ""); PlayerPrefs.SetString("tg_fb_email", email ?? ""); PlayerPrefs.Save();
        }

        static string Friendly(long code, string body, string transport)
        {
            if (transport != null) return "network: " + transport;
            if (body.Contains("EMAIL_EXISTS")) return "EMAIL_EXISTS"; if (body.Contains("INVALID_LOGIN_CREDENTIALS") || body.Contains("INVALID_PASSWORD") || body.Contains("EMAIL_NOT_FOUND")) return "INVALID_LOGIN";
            if (body.Contains("WEAK_PASSWORD")) return "WEAK_PASSWORD"; if (body.Contains("OPERATION_NOT_ALLOWED")) return "Email/Password sign-in is not enabled in Firebase console";
            return "HTTP " + code;
        }

        /// done(null) on success, otherwise an error code string.
        public IEnumerator SignIn(string mail, string password, bool create, Action<string> done)
        {
            string res = null; long code = 0; string terr = null;
            yield return Http.Send("POST", AuthUrl(create ? "signUp" : "signInWithPassword", apiKey), JsonUtility.ToJson(new FbAuthReq { email = mail, password = password }), Json(), (c, b, e) => { code = c; res = b; terr = e; });
            if (code != 200) { done(Friendly(code, res ?? "", terr)); yield break; }
            var r = JsonUtility.FromJson<FbAuthRes>(res); Store(r.idToken, r.refreshToken, r.localId, r.email ?? mail, r.expiresIn); done(null);
        }
        public void SignOut() { idToken = null; refreshToken = null; uid = null; email = null; PlayerPrefs.DeleteKey("tg_fb_refresh"); PlayerPrefs.DeleteKey("tg_fb_uid"); PlayerPrefs.DeleteKey("tg_fb_email"); PlayerPrefs.Save(); }

        IEnumerator EnsureToken(Action<string> done)
        {
            if (!SignedIn) { done("signed out"); yield break; }
            if (!string.IsNullOrEmpty(idToken) && DateTime.UtcNow < expiresAt) { done(null); yield break; }
            long code = 0; string res = null, terr = null;
            var h = new Dictionary<string, string> { ["Content-Type"] = "application/x-www-form-urlencoded" };
            yield return Http.Send("POST", "https://securetoken.googleapis.com/v1/token?key=" + apiKey, "grant_type=refresh_token&refresh_token=" + UnityEngine.Networking.UnityWebRequest.EscapeURL(refreshToken), h, (c, b, e) => { code = c; res = b; terr = e; });
            if (code != 200) { if (terr == null) SignOut(); done(Friendly(code, res ?? "", terr)); yield break; }
            var r = JsonUtility.FromJson<FbRefreshRes>(res); Store(r.id_token, r.refresh_token, r.user_id, null, r.expires_in); done(null);
        }

        /// Pull remote progress, merge (union; never loses data), push merged result.
        public IEnumerator Sync(SaveSystem save, Action<string> done)
        {
            string err = null; yield return EnsureToken(e => err = e); if (err != null) { done(err); yield break; }
            long code = 0; string res = null, terr = null;
            yield return Http.Send("GET", DocUrl(), null, Bearer(), (c, b, e) => { code = c; res = b; terr = e; });
            if (code == 200)
            {
                try { var d = JsonUtility.FromJson<FbDoc>(res); if (d?.fields?.progress != null) save.MergeFrom(JsonUtility.FromJson<SaveData>(d.fields.progress.stringValue)); }
                catch (Exception ex) { done("remote data unreadable: " + ex.Message); yield break; }
            }
            else if (code != 404) { done(Friendly(code, res ?? "", terr)); yield break; }
            var body = "{\"fields\":{\"progress\":{\"stringValue\":" + JsonString(JsonUtility.ToJson(save.Data)) + "},\"updatedUtc\":{\"stringValue\":" + JsonString(save.Data.updatedUtc) + "}}}";
            yield return Http.Send("PATCH", DocUrl() + "?updateMask.fieldPaths=progress&updateMask.fieldPaths=updatedUtc", body, Bearer(), (c, b, e) => { code = c; res = b; terr = e; });
            done(code == 200 ? null : Friendly(code, res ?? "", terr));
        }

        /// Deletes the user's cloud document and then the Firebase account itself. Local data is untouched.
        public IEnumerator DeleteAccount(Action<string> done)
        {
            string err = null; yield return EnsureToken(e => err = e); if (err != null) { done(err); yield break; }
            long code = 0; string res = null, terr = null;
            yield return Http.Send("DELETE", DocUrl(), null, Bearer(), (c, b, e) => { code = c; res = b; terr = e; });
            if (code != 200 && code != 404) { done(Friendly(code, res ?? "", terr)); yield break; }
            yield return Http.Send("POST", AuthUrl("delete", apiKey), JsonUtility.ToJson(new FbIdTokenReq { idToken = idToken }), Json(), (c, b, e) => { code = c; res = b; terr = e; });
            if (code == 200) { SignOut(); done(null); } else done(Friendly(code, res ?? "", terr));
        }

        static string JsonString(string s)
        {
            var sb = new System.Text.StringBuilder("\"");
            foreach (char c in s)
            {
                switch (c) { case '"': sb.Append("\\\""); break; case '\\': sb.Append("\\\\"); break; case '\n': sb.Append("\\n"); break; case '\r': sb.Append("\\r"); break; case '\t': sb.Append("\\t"); break;
                    default: if (c < ' ') sb.Append("\\u" + ((int)c).ToString("x4")); else sb.Append(c); break; }
            }
            return sb.Append('"').ToString();
        }
    }
}
