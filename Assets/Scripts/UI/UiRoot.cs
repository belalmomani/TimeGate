using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using TimeGate.Core;
using TimeGate.Data;
using TimeGate.Dialogue;
using TimeGate.Services;
using TimeGate.World;
namespace TimeGate.UI
{
    public class UiRoot
    {
        readonly GameApp app; readonly VisualElement root; VisualElement menu, hud, fade, popup;
        readonly Font font; Label toast, questLabel; VisualElement interactBtn; Label interactLbl; VisualElement joyKnob;
        public bool FontMissing => font == null;

        public UiRoot(GameApp a)
        {
            app = a; font = Resources.Load<Font>("Fonts/DejaVuSans");
            var go = new GameObject("UI"); go.transform.SetParent(a.transform); go.SetActive(false);
            var doc = go.AddComponent<UIDocument>(); var ps = ScriptableObject.CreateInstance<PanelSettings>();
            ps.scaleMode = PanelScaleMode.ScaleWithScreenSize; ps.referenceResolution = new Vector2Int(1920, 1080);
            ps.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight; ps.match = 0.5f;
            ps.themeStyleSheet = ScriptableObject.CreateInstance<ThemeStyleSheet>();
            doc.panelSettings = ps; go.SetActive(true); root = doc.rootVisualElement;
            root.style.flexGrow = 1; root.pickingMode = PickingMode.Ignore;
            fade = Abs(); fade.pickingMode = PickingMode.Ignore; fade.style.opacity = 0; fade.style.backgroundColor = Palette.Energy;
            root.Add(fade);
        }

        // ---------- helpers ----------
        static VisualElement Abs() { var e = new VisualElement(); var s = e.style; s.position = Position.Absolute; s.left = 0; s.right = 0; s.top = 0; s.bottom = 0; return e; }
        void Txt(VisualElement e, int size, Color c, bool bold = false, bool align = true)
        {
            var s = e.style; s.fontSize = size; s.color = c; s.unityFontStyleAndWeight = bold ? FontStyle.Bold : FontStyle.Normal;
            if (font != null) s.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(font));
            s.unityTextGenerator = TextGeneratorType.Advanced; s.whiteSpace = WhiteSpace.Normal;
            if (align) s.unityTextAlign = Loc.IsRtl ? TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
        }
        Label Lbl(string t, int size = 32, Color? c = null, bool bold = false) { var l = new Label(t); Txt(l, size, c ?? Palette.Text, bold); l.pickingMode = PickingMode.Ignore; return l; }
        VisualElement Btn(string text, Action on, bool primary = true, bool enabled = true)
        {
            var b = new VisualElement(); var s = b.style;
            s.backgroundColor = primary ? new Color(0.04f, 0.3f, 0.52f, 0.95f) : new Color(0.08f, 0.12f, 0.2f, 0.95f);
            s.borderTopWidth = s.borderBottomWidth = s.borderLeftWidth = s.borderRightWidth = 2;
            s.borderTopColor = s.borderBottomColor = s.borderLeftColor = s.borderRightColor = primary ? Palette.Gold : Palette.EnergyDeep;
            s.borderTopLeftRadius = s.borderTopRightRadius = s.borderBottomLeftRadius = s.borderBottomRightRadius = 16;
            s.paddingTop = s.paddingBottom = 16; s.paddingLeft = s.paddingRight = 24; s.marginTop = 10; s.alignItems = Align.Center;
            var l = Lbl(text, 32, Palette.Text, true); l.style.unityTextAlign = TextAnchor.MiddleCenter; b.Add(l);
            if (enabled) b.RegisterCallback<ClickEvent>(_ => on()); else b.style.opacity = 0.45f;
            return b;
        }
        VisualElement SmallBtn(string text, Action on)
        {
            var b = Btn(text, on, false); b.style.paddingTop = b.style.paddingBottom = 8; b.style.paddingLeft = b.style.paddingRight = 16; b.style.marginTop = 0; b.style.marginLeft = b.style.marginRight = 6; return b;
        }
        VisualElement NewPanel(string title, Action back)
        {
            if (menu != null) menu.RemoveFromHierarchy();
            menu = Abs(); menu.style.flexDirection = Loc.IsRtl ? FlexDirection.RowReverse : FlexDirection.Row; root.Add(menu); menu.BringToFront(); fade.BringToFront();
            var panel = new VisualElement(); var s = panel.style; s.width = 820; s.height = Length.Percent(100); s.backgroundColor = Palette.Panel; s.paddingTop = 36; s.paddingBottom = 24; s.paddingLeft = s.paddingRight = 36;
            menu.Add(panel);
            var t = Lbl(title, 54, Palette.Gold, true); panel.Add(t);
            var sc = new ScrollView(ScrollViewMode.Vertical); sc.style.flexGrow = 1; sc.style.marginTop = 12; sc.verticalScrollerVisibility = ScrollerVisibility.Hidden; panel.Add(sc);
            if (back != null) panel.Add(Btn(Loc.T("back"), back, false));
            return sc;
        }

        // ---------- screens ----------
        public void ShowHome()
        {
            HideHud(); var p = NewPanel(Loc.T("title"), null);
            p.Add(Lbl(Loc.T("subtitle"), 30, Palette.Energy));
            if (FontMissing) p.Add(Lbl("Arabic font missing: add a TTF at Assets/Resources/Fonts", 24, Color.yellow));
            p.Add(Btn(Loc.T("random"), RandomScreen));
            p.Add(Btn(Loc.T("byCountry"), () => CountryList(ShowHome)));
            p.Add(Btn(Loc.T("byCiv"), () => CivList(ShowHome)));
            p.Add(Btn(Loc.T("byPeriod"), PeriodScreen));
            p.Add(Btn(Loc.T("achievements"), AchievementsScreen, false));
            p.Add(Btn(Loc.T("history"), HistoryScreen, false));
            p.Add(Btn(Loc.T("favorites"), FavoritesScreen, false));
            p.Add(Btn(Loc.T("account"), AccountScreen, false));
            p.Add(Btn(Loc.T("settings"), SettingsScreen, false));
            p.Add(Btn(Loc.T("about"), AboutScreen, false));
        }

        string Period(PackManifest p)
        {
            if (p.periodConfidence == "unknown") return Loc.T("periodUnknown");
            string F(int y) => y < 0 ? (Loc.IsRtl ? (-y) + " ق.م" : (-y) + " BCE") : (Loc.IsRtl ? y + " م" : y + " CE");
            string conf = p.periodConfidence == "approximate" ? (Loc.IsRtl ? " (تقريبًا)" : " (approx.)") : "";
            return F(p.yearFrom) + " – " + F(p.yearTo) + conf;
        }
        string Conf(string c) => c == "documented" ? Loc.T("conf_documented") : c == "approximate" ? Loc.T("conf_approximate") : Loc.T("conf_artistic");

        void RandomScreen()
        {
            var p = NewPanel(Loc.T("random"), ShowHome); var result = new VisualElement(); p.Add(result);
            (PackManifest pack, SiteDef site)? picked = null;
            void Roll()
            {
                result.Clear();
                var l = Lbl(Loc.T("rolling"), 40, Palette.Energy, true); result.Add(l);
                app.RollDice(r =>
                {
                    result.Clear(); picked = r;
                    if (r == null) { result.Add(Lbl(Loc.T("noPlayable"), 32)); return; }
                    var (pk, st) = r.Value;
                    result.Add(Lbl(Loc.Pick(st.name), 46, Palette.Gold, true));
                    result.Add(Lbl(Loc.T("country") + ": " + Loc.Pick(pk.country), 30));
                    result.Add(Lbl(Loc.T("civ") + ": " + Loc.Pick(pk.civilization), 30));
                    result.Add(Lbl(Loc.T("period") + ": " + Period(pk), 30));
                    result.Add(Btn(Loc.T("confirm"), () => app.BeginTravel(pk, st)));
                    result.Add(Btn(Loc.T("reroll"), Roll, false));
                });
            }
            p.Add(Lbl("", 10)); result.Add(Btn(Loc.T("roll"), Roll));
        }

        void CountryList(Action back)
        {
            var p = NewPanel(Loc.T("byCountry"), back);
            foreach (var g in app.Registry.Packs.GroupBy(x => x.countryCode))
            {
                var first = g.First(); var list = g.SelectMany(pk => pk.sites.Select(s => (pk, s))).ToList();
                p.Add(Btn(Loc.Pick(first.country) + "  (" + list.Count + ")", () => SiteList(Loc.Pick(first.country), list, () => CountryList(back))));
            }
        }
        void CivList(Action back)
        {
            var p = NewPanel(Loc.T("byCiv"), back);
            foreach (var g in app.Registry.Packs.GroupBy(x => Loc.Pick(x.civilization)))
            {
                var first = g.First(); var list = g.SelectMany(pk => pk.sites.Select(s => (pk, s))).ToList();
                p.Add(Btn(g.Key + " — " + Period(first), () => SiteList(g.Key, list, () => CivList(back))));
            }
        }
        void SiteList(string title, List<(PackManifest pack, SiteDef site)> list, Action back)
        {
            var p = NewPanel(title, back);
            foreach (var (pk, st) in list)
            {
                bool ok = DestinationRegistry.IsAvailable(pk, st);
                p.Add(Btn(Loc.Pick(st.name) + (ok ? "" : "  [" + Loc.T("inDev") + "]"), () => SiteDetail(pk, st, () => SiteList(title, list, back)), ok, true));
            }
        }
        void SiteDetail(PackManifest pk, SiteDef st, Action back)
        {
            var p = NewPanel(Loc.Pick(st.name), back); bool ok = DestinationRegistry.IsAvailable(pk, st);
            p.Add(Lbl(Loc.T("country") + ": " + Loc.Pick(pk.country) + "  |  " + Loc.T("civ") + ": " + Loc.Pick(pk.civilization), 26, Palette.Energy));
            p.Add(Lbl(Loc.T("period") + ": " + Period(pk), 26, Palette.Energy));
            if (st.region != null && !string.IsNullOrEmpty(st.region.ar)) p.Add(Lbl(Loc.T("region") + ": " + Loc.Pick(st.region), 26, Palette.Energy));
            p.Add(Lbl("● " + Conf(st.confidence), 26, Palette.Gold));
            p.Add(Lbl(Loc.Pick(st.summary), 30));
            if (ok) { p.Add(Lbl(Loc.T("placeholderNote"), 24, Color.yellow)); p.Add(Lbl(Loc.T("bundled"), 22, Palette.Energy)); }
            else p.Add(Lbl(Loc.T("inDev"), 28, Color.yellow));
            if (st.references != null && st.references.Count > 0)
            {
                p.Add(Lbl(Loc.T("references"), 34, Palette.Gold, true));
                foreach (var r in st.references)
                {
                    p.Add(Lbl("• " + r.title + (r.verified ? "" : " " + Loc.T("unverified")), 24));
                    var url = r.url; p.Add(SmallBtn("↗ " + url, () => Application.OpenURL(url)));
                }
            }
            p.Add(Btn(Loc.T("travel"), () => app.BeginTravel(pk, st), true, ok));
            bool fav = app.Save.IsFavorite(st.id);
            p.Add(Btn(fav ? Loc.T("unfav") : Loc.T("fav"), () => { app.Save.ToggleFavorite(st.id); SiteDetail(pk, st, back); }, false));
        }
        int periodYear = 100;
        static string YearStr(int y) => y < 0 ? (Loc.IsRtl ? (-y) + " ق.م" : (-y) + " BCE") : (Loc.IsRtl ? y + " م" : y + " CE");
        void PeriodScreen()
        {
            var p = NewPanel(Loc.T("byPeriod"), ShowHome);
            p.Add(Lbl(Loc.T("year") + ": " + YearStr(periodYear), 46, Palette.Gold, true));
            var row = new VisualElement(); row.style.flexDirection = FlexDirection.Row; row.style.flexWrap = Wrap.Wrap; row.style.justifyContent = Justify.Center;
            foreach (int step in new[] { -500, -100, -10, 10, 100, 500 })
            {
                int st0 = step;
                var b = SmallBtn((st0 > 0 ? "+" : "") + st0, () => { periodYear = Mathf.Clamp(periodYear + st0, -3000, 2100); if (periodYear == 0) periodYear = st0 > 0 ? 1 : -1; PeriodScreen(); });
                b.style.marginBottom = 6; row.Add(b);
            }
            p.Add(row); p.Add(Lbl(Loc.T("yearNote"), 22, Color.yellow));
            var list = app.Registry.AllSites().Where(t => t.pack.yearFrom <= periodYear && periodYear <= t.pack.yearTo).ToList();
            if (list.Count == 0) p.Add(Lbl(Loc.T("noSitesYear"), 28));
            foreach (var (pk, st) in list)
            {
                bool ok = DestinationRegistry.IsAvailable(pk, st);
                p.Add(Btn(Loc.Pick(st.name) + " — " + Loc.Pick(pk.civilization) + (ok ? "" : "  [" + Loc.T("inDev") + "]"), () => SiteDetail(pk, st, PeriodScreen), ok, true));
            }
        }
        void AchievementsScreen()
        {
            var p = NewPanel(Loc.T("achievements"), ShowHome); var d = app.Save.Data;
            void Row(bool done, string ar, string en) { p.Add(Lbl((done ? "★ " : "☆ ") + (Loc.IsRtl ? ar : en), 30, done ? Palette.Gold : Palette.Text)); }
            Row(d.visits.Count >= 1, "أول رحلة عبر البوابة", "First trip through the gate");
            Row(d.visits.Count >= 5, "خمس رحلات", "Five trips");
            Row(d.visits.Any(v => v.siteId == "petra"), "زيارة البتراء", "Visited Petra");
            Row(d.visits.Any(v => v.siteId == "jerash"), "زيارة جرش", "Visited Jerash");
            Row(d.inspectedPois.Contains("petra_khazneh"), "فحص الخزنة", "Inspected the Treasury");
            Row(d.completedQuests.Contains("petra_walk_siq"), "مهمة البتراء", "Petra quest");
            Row(d.completedQuests.Contains("jerash_walk"), "مهمة جرش", "Jerash quest");
            Row(d.favorites.Count >= 1, "أول موقع مفضّل", "First favorite");
        }
        void HistoryScreen()
        {
            var p = NewPanel(Loc.T("history"), ShowHome);
            if (app.Save.Data.visits.Count == 0) p.Add(Lbl(Loc.T("noVisits"), 30));
            foreach (var v in Enumerable.Reverse(app.Save.Data.visits).Take(50))
            {
                var site = app.Registry.FindSite(v.siteId); var pk = app.Registry.FindPackOfSite(v.siteId); if (site == null) continue;
                var t = v.utc; if (DateTime.TryParse(v.utc, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dt)) t = dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
                p.Add(Btn(Loc.Pick(site.name) + "  —  " + t, () => SiteDetail(pk, site, HistoryScreen), false));
            }
        }
        void FavoritesScreen()
        {
            var p = NewPanel(Loc.T("favorites"), ShowHome);
            if (app.Save.Data.favorites.Count == 0) p.Add(Lbl(Loc.T("noFavs"), 30));
            foreach (var id in app.Save.Data.favorites)
            {
                var site = app.Registry.FindSite(id); var pk = app.Registry.FindPackOfSite(id); if (site == null) continue;
                p.Add(Btn(Loc.Pick(site.name), () => SiteDetail(pk, site, FavoritesScreen), false));
            }
        }
        void SettingsScreen()
        {
            var d = app.Save.Data; var p = NewPanel(Loc.T("settings"), ShowHome);
            p.Add(Btn(Loc.T("language"), () => { Loc.Lang = Loc.Lang == "ar" ? "en" : "ar"; d.language = Loc.Lang; app.Save.Save(); SettingsScreen(); }));
            p.Add(Lbl(Loc.T("sensitivity") + ": " + d.sensitivity.ToString("0.00"), 30));
            var row = new VisualElement(); row.style.flexDirection = FlexDirection.Row; row.style.justifyContent = Justify.Center;
            row.Add(SmallBtn("−", () => { d.sensitivity = Mathf.Max(0.25f, d.sensitivity - 0.25f); app.ApplySettings(); SettingsScreen(); }));
            row.Add(SmallBtn("+", () => { d.sensitivity = Mathf.Min(3f, d.sensitivity + 0.25f); app.ApplySettings(); SettingsScreen(); })); p.Add(row);
            p.Add(Lbl(Loc.T("volume") + ": " + Mathf.RoundToInt(d.volume * 100f) + "%", 30));
            var vrow = new VisualElement(); vrow.style.flexDirection = FlexDirection.Row; vrow.style.justifyContent = Justify.Center;
            vrow.Add(SmallBtn("−", () => { d.volume = Mathf.Max(0f, d.volume - 0.25f); app.ApplySettings(); SettingsScreen(); }));
            vrow.Add(SmallBtn("+", () => { d.volume = Mathf.Min(1f, d.volume + 0.25f); app.ApplySettings(); SettingsScreen(); })); p.Add(vrow);
            p.Add(Btn(Loc.T("invertY") + ": " + (d.invertY ? "ON" : "OFF"), () => { d.invertY = !d.invertY; app.ApplySettings(); SettingsScreen(); }, false));
            p.Add(Btn(Loc.T("reduceFx") + ": " + (d.reduceEffects ? "ON" : "OFF"), () => { d.reduceEffects = !d.reduceEffects; app.ApplySettings(); SettingsScreen(); }, false));
            string[] q = { "low", "med", "high" }; string[] qa = { "منخفضة", "متوسطة", "عالية" };
            p.Add(Btn(Loc.T("quality") + ": " + (Loc.IsRtl ? qa[d.quality] : q[d.quality]), () => { d.quality = (d.quality + 1) % 3; app.ApplySettings(); SettingsScreen(); }, false));
            p.Add(Lbl(Loc.T("services"), 34, Palette.Gold, true));
            p.Add(Lbl(ServiceHub.Ads.Status, 22)); p.Add(Lbl(ServiceHub.Cloud.Status, 22)); p.Add(Lbl(ServiceHub.Ai.Status, 22));
        }
        TextField Field(string placeholderKey, bool pw)
        {
            var tf = new TextField(); tf.isPasswordField = pw; tf.maxLength = 120; tf.label = ""; tf.style.fontSize = 28; tf.style.color = Palette.Text; tf.style.minHeight = 64; tf.style.marginTop = 8;
            if (font != null) tf.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(font));
            tf.style.unityTextGenerator = TextGeneratorType.Advanced;
            tf.schedule.Execute(() => { var input = tf.Q(className: "unity-base-text-field__input"); if (input != null) { input.style.backgroundColor = new Color(0.05f, 0.12f, 0.2f, 1f); input.style.color = Palette.Text; } });
            return tf;
        }
        bool confirmDelete;
        void AccountScreen()
        {
            var p = NewPanel(Loc.T("account"), ShowHome); var fb = ServiceHub.Firebase;
            if (fb == null) { p.Add(Lbl(Loc.T("cloudOff"), 28)); p.Add(Lbl(ServiceHub.Cloud.Status, 22, Palette.Energy)); return; }
            var status = Lbl(fb.Status, 24, Palette.Energy); p.Add(status);
            void Result(string err) { confirmDelete = false; status.text = err == null ? Loc.T("ok") : err; }
            if (!fb.SignedIn)
            {
                var em = Field("email", false); var pw = Field("password", true);
                p.Add(Lbl(Loc.T("email"), 24)); p.Add(em); p.Add(Lbl(Loc.T("password"), 24)); p.Add(pw);
                p.Add(Btn(Loc.T("signIn"), () => { status.text = Loc.T("working"); app.StartCoroutine(fb.SignIn(em.value.Trim(), pw.value, false, e => { Result(e); if (e == null) AccountScreen(); })); }));
                p.Add(Btn(Loc.T("signUp"), () => { status.text = Loc.T("working"); app.StartCoroutine(fb.SignIn(em.value.Trim(), pw.value, true, e => { Result(e); if (e == null) AccountScreen(); })); }, false));
            }
            else
            {
                p.Add(Btn(Loc.T("syncNow"), () => { status.text = Loc.T("working"); app.StartCoroutine(fb.Sync(app.Save, Result)); }));
                p.Add(Btn(Loc.T("signOut"), () => { fb.SignOut(); AccountScreen(); }, false));
                p.Add(Btn(Loc.T("deleteAcc"), () =>
                {
                    if (!confirmDelete) { confirmDelete = true; status.text = Loc.T("confirmDel"); return; }
                    status.text = Loc.T("working"); app.StartCoroutine(fb.DeleteAccount(e => { Result(e); if (e == null) AccountScreen(); }));
                }, false));
            }
        }
        void AboutScreen()
        {
            var p = NewPanel(Loc.T("about"), ShowHome);
            p.Add(Lbl("TimeGate v" + Application.version + "  (" + Application.identifier + ")", 26, Palette.Energy));
            if (ServiceHub.Supabase != null)
            {
                var info = Lbl("", 24, Palette.Energy);
                p.Add(Btn(Loc.T("updates"), () => { info.text = Loc.T("working"); app.StartCoroutine(ServiceHub.Supabase.PublishedPacks(l => info.text = string.Join("\n", l), e => info.text = e)); }, false)); p.Add(info);
            }
            p.Add(Lbl("3D geometry: procedural (no third-party models yet).\nFont: DejaVu Sans (free license; docs/DejaVu-LICENSE.txt).\nWind audio: generated in code.\nSee docs/ASSETS.md for the asset register.", 26));
        }

        // ---------- HUD ----------
        public void HideHud() { if (hud != null) { hud.RemoveFromHierarchy(); hud = null; } if (popup != null) { popup.RemoveFromHierarchy(); popup = null; } TouchInput.Reset(); }
        public void ShowHud()
        {
            if (menu != null) { menu.RemoveFromHierarchy(); menu = null; }
            HideHud(); hud = Abs(); hud.pickingMode = PickingMode.Ignore; root.Add(hud); fade.BringToFront();
            // look area
            var look = Abs(); look.style.left = Length.Percent(40); look.pickingMode = PickingMode.Position; hud.Add(look); int lookId = -1;
            look.RegisterCallback<PointerDownEvent>(e => { if (lookId != -1) return; lookId = e.pointerId; look.CapturePointer(lookId); });
            look.RegisterCallback<PointerMoveEvent>(e => { if (e.pointerId == lookId) TouchInput.LookDelta += new Vector2(e.deltaPosition.x, e.deltaPosition.y); });
            look.RegisterCallback<PointerUpEvent>(e => { if (e.pointerId == lookId) { look.ReleasePointer(lookId); lookId = -1; } });
            look.RegisterCallback<PointerCaptureOutEvent>(e => lookId = -1);
            // joystick
            var joy = new VisualElement(); var js = joy.style; js.position = Position.Absolute; js.left = 70; js.bottom = 70; js.width = 240; js.height = 240;
            js.borderTopLeftRadius = js.borderTopRightRadius = js.borderBottomLeftRadius = js.borderBottomRightRadius = 120; js.backgroundColor = new Color(0.1f, 0.5f, 0.8f, 0.18f);
            js.borderTopWidth = js.borderBottomWidth = js.borderLeftWidth = js.borderRightWidth = 3; js.borderTopColor = js.borderBottomColor = js.borderLeftColor = js.borderRightColor = new Color(0.25f, 0.8f, 1f, 0.6f);
            joyKnob = new VisualElement(); var ks = joyKnob.style; ks.position = Position.Absolute; ks.width = 100; ks.height = 100; ks.left = 70; ks.top = 70; ks.backgroundColor = new Color(0.25f, 0.8f, 1f, 0.55f);
            ks.borderTopLeftRadius = ks.borderTopRightRadius = ks.borderBottomLeftRadius = ks.borderBottomRightRadius = 50; joyKnob.pickingMode = PickingMode.Ignore; joy.Add(joyKnob); hud.Add(joy); int joyId = -1;
            void Upd(Vector3 lp) { var o = new Vector2(lp.x - 120, lp.y - 120); if (o.magnitude > 100) o = o.normalized * 100; joyKnob.style.left = 120 + o.x - 50; joyKnob.style.top = 120 + o.y - 50; TouchInput.Move = new Vector2(o.x / 100f, -o.y / 100f); }
            void End() { joyId = -1; TouchInput.Move = Vector2.zero; joyKnob.style.left = 70; joyKnob.style.top = 70; }
            joy.RegisterCallback<PointerDownEvent>(e => { if (joyId != -1) return; joyId = e.pointerId; joy.CapturePointer(joyId); Upd(e.localPosition); });
            joy.RegisterCallback<PointerMoveEvent>(e => { if (e.pointerId == joyId) Upd(e.localPosition); });
            joy.RegisterCallback<PointerUpEvent>(e => { if (e.pointerId == joyId) { joy.ReleasePointer(joyId); End(); } });
            joy.RegisterCallback<PointerCaptureOutEvent>(e => End());
            // up/down/boost
            hud.Add(Hold("▲", 70, 70, v => TouchInput.Vertical = v ? 1f : (TouchInput.Vertical > 0 ? 0 : TouchInput.Vertical), true));
            hud.Add(Hold("▼", 70, 190, v => TouchInput.Vertical = v ? -1f : (TouchInput.Vertical < 0 ? 0 : TouchInput.Vertical), true));
            hud.Add(Hold("»", 200, 70, v => TouchInput.Boost = v, true));
            hud.Add(Hold("~", 200, 190, v => TouchInput.Slow = v, true));
            // top bar
            var top = new VisualElement(); top.style.position = Position.Absolute; top.style.top = 20; top.style.left = 20; top.style.right = 20; top.style.flexDirection = FlexDirection.Row; top.pickingMode = PickingMode.Ignore; hud.Add(top);
            top.Add(SmallBtn("⟲ " + Loc.T("gate"), () => app.ReturnToGate()));
            top.Add(SmallBtn(Loc.T("camView"), () => app.ToggleView()));
            top.Add(SmallBtn("◎ " + Loc.T("camReset"), () => app.ResetCam()));
            questLabel = Lbl("", 26, Palette.Gold, true); questLabel.style.marginLeft = 16; questLabel.style.flexShrink = 1; top.Add(questLabel);
            toast = Lbl("", 34, Color.yellow, true); toast.style.position = Position.Absolute; toast.style.top = 110; toast.style.left = 0; toast.style.right = 0; toast.style.unityTextAlign = TextAnchor.MiddleCenter; hud.Add(toast);
            interactBtn = Btn("", () => app.InteractNearest(), true); interactLbl = (Label)interactBtn.ElementAt(0);
            interactBtn.style.position = Position.Absolute; interactBtn.style.bottom = 70; interactBtn.style.left = Length.Percent(38); interactBtn.style.right = Length.Percent(38); interactBtn.style.display = DisplayStyle.None; hud.Add(interactBtn);
        }
        VisualElement Hold(string t, float right, float bottom, Action<bool> on, bool fromRight)
        {
            var b = new VisualElement(); var s = b.style; s.position = Position.Absolute; s.right = right; s.bottom = bottom; s.width = 110; s.height = 110; s.alignItems = Align.Center; s.justifyContent = Justify.Center;
            s.backgroundColor = new Color(0.05f, 0.2f, 0.35f, 0.55f); s.borderTopLeftRadius = s.borderTopRightRadius = s.borderBottomLeftRadius = s.borderBottomRightRadius = 55;
            s.borderTopWidth = s.borderBottomWidth = s.borderLeftWidth = s.borderRightWidth = 2; s.borderTopColor = s.borderBottomColor = s.borderLeftColor = s.borderRightColor = Palette.Gold;
            var l = Lbl(t, 48, Palette.Text, true); l.style.unityTextAlign = TextAnchor.MiddleCenter; b.Add(l); int id = -1;
            b.RegisterCallback<PointerDownEvent>(e => { if (id != -1) return; id = e.pointerId; b.CapturePointer(id); on(true); });
            b.RegisterCallback<PointerUpEvent>(e => { if (e.pointerId == id) { b.ReleasePointer(id); id = -1; on(false); } });
            b.RegisterCallback<PointerCaptureOutEvent>(e => { if (id != -1) { id = -1; on(false); } });
            return b;
        }
        public void SetInteract(string name) { if (interactBtn == null) return; if (string.IsNullOrEmpty(name)) interactBtn.style.display = DisplayStyle.None; else { interactBtn.style.display = DisplayStyle.Flex; interactLbl.text = Loc.T("interact") + ": " + name; } }
        public void SetQuest(string s) { if (questLabel != null) questLabel.text = s; }
        public void SetToast(string s) { if (toast != null && toast.text != s) toast.text = s; }

        public void ShowPoi(PoiDef poi, QuestDef completed, Action onClose)
        {
            if (popup != null) popup.RemoveFromHierarchy();
            popup = Abs(); popup.style.backgroundColor = new Color(0, 0, 0, 0.6f); popup.style.alignItems = Align.Center; popup.style.justifyContent = Justify.Center; root.Add(popup); fade.BringToFront();
            var card = new VisualElement(); card.style.width = 1100; card.style.maxHeight = Length.Percent(88); card.style.backgroundColor = Palette.Panel; card.style.paddingTop = card.style.paddingBottom = card.style.paddingLeft = card.style.paddingRight = 36; popup.Add(card);
            var sc = new ScrollView(ScrollViewMode.Vertical); sc.verticalScrollerVisibility = ScrollerVisibility.Hidden; card.Add(sc);
            sc.Add(Lbl(Loc.Pick(poi.name), 52, Palette.Gold, true)); sc.Add(Lbl("● " + Conf(poi.confidence), 26, Palette.Energy)); sc.Add(Lbl(Loc.Pick(poi.summary), 32));
            if (completed != null) sc.Add(Lbl("★ " + Loc.T("questDone") + ": " + Loc.Pick(completed.title), 34, Palette.Gold, true));
            card.Add(Btn(Loc.T("close"), () => { popup.RemoveFromHierarchy(); popup = null; onClose(); }));
        }
        public void ShowChat(CharacterDef c, DialogueService dlg, Action onClose)
        {
            if (popup != null) popup.RemoveFromHierarchy();
            popup = Abs(); popup.style.backgroundColor = new Color(0, 0, 0, 0.6f); popup.style.alignItems = Align.Center; popup.style.justifyContent = Justify.Center; root.Add(popup); fade.BringToFront();
            var card = new VisualElement(); card.style.width = 1200; card.style.height = Length.Percent(92); card.style.backgroundColor = Palette.Panel;
            card.style.paddingTop = card.style.paddingBottom = card.style.paddingLeft = card.style.paddingRight = 28; popup.Add(card);
            card.Add(Lbl(Loc.Pick(c.name), 44, Palette.Gold, true));
            card.Add(Lbl(Loc.Pick(c.role) + " — " + Loc.T("fictional"), 24, Palette.Energy));
            card.Add(Lbl(Loc.T("chatNote"), 20, Color.yellow));
            var log = new ScrollView(ScrollViewMode.Vertical); log.style.flexGrow = 1; log.style.flexShrink = 1; log.style.marginTop = 8; log.verticalScrollerVisibility = ScrollerVisibility.Hidden; card.Add(log);
            void Add(string who, string text, Color col)
            {
                var l = Lbl(who + ": " + text, 28, col); l.style.marginTop = 8; log.Add(l);
                log.schedule.Execute(() => log.scrollOffset = new Vector2(0, 100000f));
            }
            Add(Loc.Pick(c.name), Loc.Pick(c.greeting), Palette.Gold);
            void Ask(string q)
            {
                if (string.IsNullOrWhiteSpace(q)) return; Add(Loc.T("you"), q, Palette.Text);
                var (topic, text) = dlg.Answer(c, q);
                if (topic != null) { Add(Loc.Pick(c.name), text + "  [" + Conf(topic.confidence) + "]", Palette.Gold); return; }
                if (!ServiceHub.Ai.IsConfigured) { Add(Loc.Pick(c.name), text, Palette.Gold); return; }
                Add(Loc.Pick(c.name), Loc.T("thinking"), Palette.Gold);
                app.StartCoroutine(ServiceHub.Ai.Ask(c.id, q, Loc.Lang,
                    ans => Add(Loc.Pick(c.name), ans + "  [" + Loc.T("aiTag") + "]", Palette.Gold),
                    err => Add(Loc.Pick(c.name), Loc.T("offlineAi"), Palette.Gold)));
            }
            var chips = new VisualElement(); chips.style.flexDirection = Loc.IsRtl ? FlexDirection.RowReverse : FlexDirection.Row; chips.style.flexWrap = Wrap.Wrap; chips.style.marginTop = 8; card.Add(chips);
            foreach (var t in c.topics) { var q = Loc.Pick(t.question); var chip = SmallBtn(q, () => Ask(q)); chip.style.marginBottom = 6; chips.Add(chip); }
            var row = new VisualElement(); row.style.flexDirection = Loc.IsRtl ? FlexDirection.RowReverse : FlexDirection.Row; row.style.marginTop = 8; card.Add(row);
            var tf = new TextField(); tf.maxLength = 200; tf.style.flexGrow = 1; tf.style.fontSize = 28; tf.style.color = Palette.Text; tf.style.minHeight = 70;
            if (font != null) tf.style.unityFontDefinition = new StyleFontDefinition(FontDefinition.FromFont(font));
            tf.style.unityTextGenerator = TextGeneratorType.Advanced;
            row.Add(tf);
            tf.schedule.Execute(() => { var input = tf.Q(className: "unity-base-text-field__input"); if (input != null) { input.style.backgroundColor = new Color(0.05f, 0.12f, 0.2f, 1f); input.style.color = Palette.Text; } });
            row.Add(SmallBtn(Loc.T("send"), () => { var q = tf.value; tf.value = ""; Ask(q); }));
            card.Add(Btn(Loc.T("close"), () => { popup.RemoveFromHierarchy(); popup = null; onClose(); }));
        }
        public void ShowConsent(Action<bool> result)
        {
            if (popup != null) popup.RemoveFromHierarchy();
            popup = Abs(); popup.style.backgroundColor = new Color(0, 0, 0, 0.7f); popup.style.alignItems = Align.Center; popup.style.justifyContent = Justify.Center; root.Add(popup); fade.BringToFront();
            var card = new VisualElement(); card.style.width = 1000; card.style.backgroundColor = Palette.Panel;
            card.style.paddingTop = card.style.paddingBottom = card.style.paddingLeft = card.style.paddingRight = 36; popup.Add(card);
            card.Add(Lbl(Loc.T("consentTitle"), 48, Palette.Gold, true)); card.Add(Lbl(Loc.T("consentBody"), 30));
            card.Add(Btn(Loc.T("accept"), () => { popup.RemoveFromHierarchy(); popup = null; result(true); }));
            card.Add(Btn(Loc.T("decline"), () => { popup.RemoveFromHierarchy(); popup = null; result(false); }, false));
        }
        public void SetFade(float a, Color c) { fade.style.backgroundColor = c; fade.style.opacity = a; fade.pickingMode = a > 0.01f ? PickingMode.Position : PickingMode.Ignore; }
    }
}
