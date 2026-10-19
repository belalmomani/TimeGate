# TimeGate | بوابة الزمن
Android 3D time-travel exploration game. Package id (fixed forever): `com.timegate.timetravel`.
You fly as a blue energy spirit through a time gate into historical sites. Engine: **Unity 6 LTS** (built-in render pipeline, UI Toolkit), built in CI with GameCI.

## Layout
`Assets/Scripts/{Core,Data,Travel,UI,World,Services}` code · `Assets/Resources/Packs/*.json` destination packs (data separate from code) · `Assets/Editor/BuildScript.cs` CI entry · `tools/validate_repo.py` pre-build checks · `supabase/` SQL · `docs/` status, assets, services.

## Add a destination
1. Add `Assets/Resources/Packs/<id>.json` (status `planned` until playable). 2. Implement a world builder, register it in `World/WorldFactory.cs`. 3. Set `playable:true`, `worldBuilder`. Random travel and menus pick it up automatically; planned/unbuilt sites are shown as "in development" and never selectable.

## Build (CI)
Secrets: `UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`; optional signing: `ANDROID_KEYSTORE_NAME/BASE64/PASS`, `ANDROID_KEYALIAS_NAME/PASS`. Push to `main` → APK artifact; push tag `v0.1.0` → GitHub Release. Bump `version.json` (versionCode strictly increasing; same keystore forever). Unity version is in `ProjectSettings/ProjectVersion.txt` — set it to the exact version your other GameCI project uses.

## Honest status → docs/STATUS.md
