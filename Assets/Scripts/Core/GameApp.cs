using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using TimeGate.Data;
using TimeGate.Dialogue;
using TimeGate.Services;
using TimeGate.Travel;
using TimeGate.UI;
using TimeGate.World;
namespace TimeGate.Core
{
    public class GameApp : MonoBehaviour
    {
        public static GameApp I;
        public SaveSystem Save = new SaveSystem(); public DestinationRegistry Registry = new DestinationRegistry();
        public TravelService Travel; public UiRoot Ui;
        Camera cam; FlightCamera flight; GameObject gateRoom; DiceController dice;
        enum State { Gate, Flying } State state;
        SpiritController spirit; SpiritVisual spiritVis; WorldResult world; SiteDef curSite; PackManifest curPack; QuestService quests;
        PoiRuntime nearPoi; NpcRuntime nearNpc; bool busy; public DialogueService Dialogue = new DialogueService();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot() { if (I != null) return; var go = new GameObject("TimeGateApp"); DontDestroyOnLoad(go); go.AddComponent<GameApp>(); }

        void Awake()
        {
            I = this; Screen.sleepTimeout = SleepTimeout.NeverSleep;
            Save.Load(); Loc.Lang = Save.Data.language; Registry.Load(); ServiceHub.LoadRuntimeConfig(); Travel = new TravelService(Registry);
            var cgo = new GameObject("Main Camera"); cgo.tag = "MainCamera"; cam = cgo.AddComponent<Camera>(); cgo.AddComponent<AudioListener>();
            cam.nearClipPlane = 0.1f; cam.farClipPlane = 1500f; cam.fieldOfView = 62f; cam.clearFlags = CameraClearFlags.Skybox; cam.backgroundColor = Palette.Night;
            flight = cgo.AddComponent<FlightCamera>(); flight.enabled = false;
            Ui = new UiRoot(this); ApplySettings(); ShowGate();
        }

        public void ApplySettings()
        {
            var d = Save.Data; Application.targetFrameRate = d.quality == 0 ? 30 : 60;
            AudioListener.volume = Mathf.Clamp01(d.volume);
            QualitySettings.shadows = d.quality == 0 ? ShadowQuality.Disable : ShadowQuality.All;
            QualitySettings.shadowDistance = d.quality == 2 ? 120 : 60;
            if (flight != null) { flight.sensitivity = d.sensitivity; flight.invertY = d.invertY; }
            Save.Save();
        }

        void ShowGate()
        {
            state = State.Gate; nearPoi = null; flight.enabled = false;
            if (world != null) Destroy(world.root); world = null; if (spirit != null) Destroy(spirit.gameObject); spirit = null;
            if (gateRoom != null) Destroy(gateRoom); gateRoom = GateRoom.Build(out dice);
            cam.clearFlags = CameraClearFlags.SolidColor; cam.transform.SetPositionAndRotation(new Vector3(0, 3f, -12f), Quaternion.LookRotation(new Vector3(0, 4.5f, 3f) - new Vector3(0, 3f, -12f)));
            Ui.ShowHome(); Ui.SetFade(0, Palette.Energy);
            if (ServiceHub.Ads.IsConfigured && Save.Data.adsConsent == 0) Ui.ShowConsent(ok => { Save.Data.adsConsent = ok ? 1 : 2; Save.Save(); });
        }

        void Update()
        {
            if (state == State.Gate) { cam.transform.position = new Vector3(Mathf.Sin(Time.time * 0.25f) * 1.2f, 3f + Mathf.Sin(Time.time * 0.17f) * 0.3f, -12f); return; }
            if (state != State.Flying || spirit == null) return;
            // nearest POI
            nearPoi = null; nearNpc = null; float best = float.MaxValue; string label = null;
            foreach (var p in world.pois) { float d = Vector3.Distance(spirit.transform.position, p.pos); if (d <= p.def.radius && d < best) { best = d; nearPoi = p; label = Loc.Pick(p.def.name); } }
            foreach (var n in world.npcs) { float d = Vector3.Distance(spirit.transform.position, n.t.position); if (d <= n.def.radius && d < best) { best = d; nearNpc = n; nearPoi = null; label = Loc.Pick(n.def.name); } }
            Ui.SetInteract(label);
            Ui.SetToast(spirit.Clamped ? Loc.T("edge") : "");
            var q = quests.Active; if (q != null) { var pr = quests.Progress(q); Ui.SetQuest(Loc.T("quest") + ": " + Loc.Pick(q.title) + " (" + pr.done + "/" + pr.total + ")"); } else Ui.SetQuest("★ " + Loc.T("questDone"));
        }

        public void RollDice(Action<(PackManifest, SiteDef)?> done) { if (!busy) StartCoroutine(RollRoutine(done)); }
        IEnumerator RollRoutine(Action<(PackManifest, SiteDef)?> done)
        {
            busy = true; var pick = Travel.RollRandom(); dice.gameObject.SetActive(true);
            yield return dice.Roll(UnityEngine.Random.Range(0, 6)); yield return new WaitForSeconds(0.4f); dice.gameObject.SetActive(false);
            busy = false; done(pick == null ? ((PackManifest, SiteDef)?)null : (pick.Value.pack, pick.Value.site));
        }

        public void BeginTravel(PackManifest p, SiteDef s) { if (!busy && DestinationRegistry.IsAvailable(p, s)) StartCoroutine(TravelRoutine(p, s)); }
        IEnumerator TravelRoutine(PackManifest p, SiteDef s)
        {
            busy = true; yield return Fade(0, 1, 0.9f);
            Ui.HideHud(); if (gateRoom != null) Destroy(gateRoom); gateRoom = null;
            curPack = p; curSite = s; world = WorldFactory.Build(s.worldBuilder, s);
            var sgo = new GameObject("Spirit"); sgo.transform.position = world.spawn; sgo.transform.rotation = Quaternion.Euler(0, world.spawnYaw, 0);
            spiritVis = sgo.AddComponent<SpiritVisual>(); spiritVis.Init(Save.Data.reduceEffects);
            spirit = sgo.AddComponent<SpiritController>(); spirit.worldBounds = world.bounds; spirit.safePoint = world.spawn; spirit.cam = flight;
            cam.clearFlags = CameraClearFlags.Skybox; flight.Setup(sgo.transform, spiritVis, world.spawnYaw); flight.enabled = true; flight.sensitivity = Save.Data.sensitivity; flight.invertY = Save.Data.invertY;
            quests = new QuestService(Save, s); Save.AddVisit(p.id, s.id); Save.Data.tripsSinceAd++; state = State.Flying;
            Ui.ShowHud(); yield return Fade(1, 0, 0.9f); busy = false;
        }
        IEnumerator Fade(float a, float b, float t)
        { for (float k = 0; k < 1f; k += Time.deltaTime / t) { Ui.SetFade(Mathf.Lerp(a, b, k), Color.Lerp(Palette.Energy, Color.white, 0.4f)); yield return null; } Ui.SetFade(b, Palette.Energy); }

        public void ReturnToGate() { if (!busy) StartCoroutine(ReturnRoutine()); }
        IEnumerator ReturnRoutine() { busy = true; TouchInput.Reset(); yield return Fade(0, 1, 0.7f); Ui.HideHud(); ShowGate(); yield return Fade(1, 0, 0.7f); busy = false; AdPolicy.TryShow(Save, true, Time.realtimeSinceStartup); }

        public void InteractNearest()
        {
            if (spirit == null) return;
            if (nearNpc != null) { spirit.frozen = true; TouchInput.Reset(); Ui.ShowChat(nearNpc.def, Dialogue, () => { if (spirit != null) spirit.frozen = false; }); return; }
            if (nearPoi == null) return;
            var poi = nearPoi.def; var completed = quests.Inspect(poi); spirit.frozen = true; TouchInput.Reset();
            Ui.ShowPoi(poi, completed, () => { if (spirit != null) spirit.frozen = false; });
        }
        public void ToggleView() { if (flight != null) flight.SetFirstPerson(!flight.firstPerson); }
        public void ResetCam() { if (flight != null) flight.ResetView(); }
    }
}
