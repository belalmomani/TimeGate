# Services — status: NOTHING CONNECTED
Source of truth (no duplication): **Firebase** = auth + per-user progress/favorites sync; **Supabase** = historical content tables (`supabase/migrations/0001_init.sql`, RLS: read-only published content for anon). AI guide = your secure proxy only.
| Service | Needs from you | Where it goes |
|---|---|---|
| Firebase | `google-services.json` + Unity Firebase SDK (pinned version) | local / CI secret, never committed |
| Supabase | project URL + **anon** key; apply the SQL | `.env` / CI secret; never the service_role key |
| Start.io | real App ID + official Unity plugin (use test ads in dev) | `.env.example` lists the name |
| AI guide | URL of your server proxy holding the LLM key | `AI_PROXY_URL` |
Code stubs: `Assets/Scripts/Services/Services.cs` (Null* implementations; Settings screen shows "not configured").
Ads policy: never during flight or dialogue; menu/interstitial between trips only; consent flow required before enabling.
