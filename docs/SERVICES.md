# Services — code written (REST), NOT connected or tested
Source of truth (no duplication): **Firebase** = auth + per-user progress/favorites sync; **Supabase** = historical content tables (`supabase/migrations/0001_init.sql`, RLS: read-only published content for anon). AI guide = your secure proxy only.
| Service | Needs from you | Where it goes |
|---|---|---|
| Firebase | `google-services.json` + Unity Firebase SDK (pinned version) | local / CI secret, never committed |
| Supabase | project URL + **anon** key; apply the SQL | `.env` / CI secret; never the service_role key |
| Start.io | real App ID + official Unity plugin (use test ads in dev) | `.env.example` lists the name |
| AI guide | URL of your server proxy holding the LLM key | `AI_PROXY_URL` |
Code stubs: `Assets/Scripts/Services/Services.cs` (Null* implementations; Settings screen shows "not configured").
Ads policy: never during flight or dialogue; menu/interstitial between trips only; consent flow required before enabling.

## Implemented in code (untested until first build)
- **Firebase via REST** (`Services/FirebaseRestSync.cs`): email/password sign-in/up/out, token refresh, Firestore progress sync (union merge, nothing lost), account + cloud data deletion. Needs only **Web API key + project id** (public values) — no SDK, no google-services.json. Analytics/Crashlytics are NOT included (they need the SDK).
- **Supabase via REST** (`Services/SupabaseClient.cs`): read-only published content with the anon key.
- Values reach the app through `Assets/Resources/runtime_config.json`, generated in CI from GitHub secrets: `FIREBASE_API_KEY`, `FIREBASE_PROJECT_ID`, `SUPABASE_URL`, `SUPABASE_ANON_KEY`, `AI_PROXY_URL` (all optional; missing = feature off).
- Firebase console setup: Authentication → enable Email/Password; Firestore → create database; paste `firebase/firestore.rules` and Publish.
- Known limit: the refresh token is kept in PlayerPrefs (not encrypted).
