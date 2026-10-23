using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace TimeGate.Data
{
    [Serializable] public class VisitRecord { public string packId; public string siteId; public string utc; }
    [Serializable] public class SaveData
    {
        public int schemaVersion = 1;
        public string updatedUtc = "";
        public float volume = 1f;
        public int adsConsent = 0;   // 0 unknown, 1 granted, 2 declined
        public int tripsSinceAd = 0;
        public string language = "ar";
        public string lastSiteId = "";
        public List<VisitRecord> visits = new List<VisitRecord>();
        public List<string> favorites = new List<string>();
        public List<string> inspectedPois = new List<string>();
        public List<string> completedQuests = new List<string>();
        public float sensitivity = 1f;
        public bool invertY = false;
        public bool reduceEffects = false;
        public int quality = 1; // 0 low, 1 medium, 2 high
    }

    public class SaveSystem
    {
        const int CurrentSchema = 1;
        string Path_ => Path.Combine(Application.persistentDataPath, "save.json");
        public SaveData Data = new SaveData();

        public void Load()
        {
            try
            {
                if (!File.Exists(Path_)) return;
                var d = JsonUtility.FromJson<SaveData>(File.ReadAllText(Path_));
                if (d == null) throw new Exception("null");
                Migrate(d);
                Data = d;
            }
            catch (Exception e)
            {
                Debug.LogWarning("[TimeGate] Save unreadable, keeping backup: " + e.Message);
                try { File.Copy(Path_, Path_ + ".corrupt-" + DateTime.UtcNow.Ticks, false); } catch { }
                Data = new SaveData();
            }
        }

        static void Migrate(SaveData d)
        {
            // schemaVersion 1 is the first schema; future migrations go here, never deleting progress.
            if (d.schemaVersion < CurrentSchema) d.schemaVersion = CurrentSchema;
        }

        public void Save()
        {
            Data.updatedUtc = DateTime.UtcNow.ToString("o");
            try
            {
                var tmp = Path_ + ".tmp";
                File.WriteAllText(tmp, JsonUtility.ToJson(Data));
                if (File.Exists(Path_)) File.Copy(Path_, Path_ + ".bak", true);
                File.Copy(tmp, Path_, true);
                File.Delete(tmp);
            }
            catch (Exception e) { Debug.LogWarning("[TimeGate] Save failed (storage full?): " + e.Message); }
        }

        public void AddVisit(string packId, string siteId)
        {
            Data.visits.Add(new VisitRecord { packId = packId, siteId = siteId, utc = DateTime.UtcNow.ToString("o") });
            if (Data.visits.Count > 200) Data.visits.RemoveAt(0);
            Data.lastSiteId = siteId; Save();
        }
        /// Conflict policy: union of progress (nothing is ever lost); settings come from whichever side was saved last.
        public void MergeFrom(SaveData r)
        {
            if (r == null) return;
            foreach (var f in r.favorites) if (!Data.favorites.Contains(f)) Data.favorites.Add(f);
            foreach (var f in r.inspectedPois) if (!Data.inspectedPois.Contains(f)) Data.inspectedPois.Add(f);
            foreach (var f in r.completedQuests) if (!Data.completedQuests.Contains(f)) Data.completedQuests.Add(f);
            var seen = new HashSet<string>(); foreach (var v in Data.visits) seen.Add(v.packId + "|" + v.siteId + "|" + v.utc);
            foreach (var v in r.visits) if (seen.Add(v.packId + "|" + v.siteId + "|" + v.utc)) Data.visits.Add(v);
            Data.visits.Sort((a, b) => string.CompareOrdinal(a.utc, b.utc));
            if (Data.visits.Count > 200) Data.visits.RemoveRange(0, Data.visits.Count - 200);
            if (string.CompareOrdinal(r.updatedUtc, Data.updatedUtc) > 0)
            { Data.language = r.language; Data.sensitivity = r.sensitivity; Data.invertY = r.invertY; Data.reduceEffects = r.reduceEffects; Data.quality = r.quality; Data.volume = r.volume; Data.lastSiteId = r.lastSiteId; }
            Save();
        }
        public bool IsFavorite(string siteId) => Data.favorites.Contains(siteId);
        public void ToggleFavorite(string siteId)
        {
            if (!Data.favorites.Remove(siteId)) Data.favorites.Add(siteId);
            Save();
        }
    }
}
