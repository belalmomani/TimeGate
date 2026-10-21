# AI guide proxy (not implemented here — you host it)
APK never contains an LLM key. Put `{"aiProxyUrl":"https://your-server/guide"}` in `Assets/Resources/runtime_config.json` (https only; URL is not secret).
**Request** `POST` JSON `{characterId, question, lang}` → **Response** `{ "answer": "..." }`.
Server rules: LLM key server-side only; per-IP/user rate limit and daily cost cap; question ≤ 300 chars; answers ≤ 600 chars; system prompt: stay in character, answer only from the pack's reviewed references, say "unknown/debated" instead of inventing, never fabricate quotes or present fiction as fact; no storing conversations without consent.
Without this file the game uses offline topics only and says so.
