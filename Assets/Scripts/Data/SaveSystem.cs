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
        public bool IsFavorite(string siteId) => Data.favorites.Contains(siteId);
        public void ToggleFavorite(string siteId)
        {
            if (!Data.favorites.Remove(siteId)) Data.favorites.Add(siteId);
            Save();
        }
    }
}
