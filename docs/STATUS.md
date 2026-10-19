# STATUS — Phase 1 report (honest)

## Environment findings
No network (cannot reach GitHub/asset sites), no Unity, no Android SDK, no Gradle, no C# compiler. Therefore: **nothing has been compiled, run, or built; no APK exists; nothing was pushed to GitHub; no assets were downloaded.**

## Engine decision
Unity 6 LTS: Start.io and Firebase both ship Unity SDKs, GameCI builds APKs on GitHub Actions without Android Studio, and it matches the owner's other Unity/GitHub workflow. (Godot was the alternative; it has no first-party Start.io/Firebase SDKs.)

## Written (source only)
- Spirit flight (joystick, camera drag, up/down, boost/slow, sphere-cast collision + slide, ground guard, world bounds, 1st/3rd person, reset cam), procedural energy-spirit visual.
- Time-gate room (3D ring, swirling portal, dice with pips + roll animation).
- Travel: random (only truly available sites), by country, by civilization; site detail with confidence label + references; favorites/history; settings; AR/EN with RTL layout.
- Save system (schema-versioned, atomic write, backups, never auto-deletes), quest service (inspect POIs → quest completes once).
- Petra prototype: procedural Siq canyon + plaza, Treasury facade from primitives, 2 POIs, 1 quest, procedural wind. **Placeholder geometry = artistic reconstruction**, labelled in-app.
- Icons: Android mipmaps + adaptive foreground + Play 512 (branding/), 1024 source in Assets/Icons.
- CI workflow, validator (`python3 tools/validate_repo.py` → passes), Supabase SQL (untested), docs.

## Tested
- ✅ Repo validator (JSON packs, builder registration, secrets scan, brace balance) — run locally, passes.
- ❌ Unity compile — NOT run. Likeliest first errors: UI Toolkit API names (`unityTextGenerator`, `FontDefinition.FromFont`) if the Unity version differs; fix on first CI log.
- ❌ Everything else (flight feel, UI, Arabic shaping, performance, install, update) untested.

## Not done / blockers needing you
1. GitHub repo URL + write access (or push this folder yourself).
2. Unity CI secrets (`UNITY_LICENSE/EMAIL/PASSWORD`) and exact Unity 6 version (currently guessed `6000.0.58f2` — unverified).
3. Android keystore (same as your other app is NOT required; make a new one and keep it forever).
4. Firebase `google-services.json`, Supabase URL + anon key, Start.io App ID, AI proxy URL.
5. Licensed assets: none downloaded (see docs/ASSETS.md). Reference URLs in the Petra pack are marked `verified:false` until checked.
6. Not yet implemented: first-person NPC/characters + chat, 'by year' travel, pack download manager, Jerash/Amman worlds, ads/Firebase/Supabase wiring, audio mixer, localization of tutorial, achievements screen.
7. Arabic text relies on Unity 6 UI Toolkit advanced text generator + bundled DejaVu Sans; verify shaping on device.

## Next step
Push → read the first GitHub Actions log → fix compile errors → install APK → report real flight/UI results.
