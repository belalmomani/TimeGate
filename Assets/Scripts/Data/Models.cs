using System;
using System.Collections.Generic;
namespace TimeGate.Data
{
    [Serializable] public class LocalizedText { public string ar; public string en; }
    [Serializable] public class ReferenceDef { public string title; public string url; public string retrieved; public bool verified; }
    [Serializable] public class PoiDef
    {
        public string id; public LocalizedText name; public LocalizedText summary;
        public string confidence; // documented | approximate | artistic_reconstruction
        public float x, y, z, radius;
    }
    [Serializable] public class QuestDef { public string id; public LocalizedText title; public LocalizedText hint; public List<string> objectivePoiIds; }
    [Serializable] public class TopicDef { public List<string> keywords; public LocalizedText question; public LocalizedText answer; public string confidence; }
    [Serializable] public class CharacterDef
    {
        public string id; public LocalizedText name; public LocalizedText role; public LocalizedText greeting; public LocalizedText fallback;
        public float x, z, radius; public List<TopicDef> topics;
    }
    [Serializable] public class SiteDef
    {
        public string id; public LocalizedText name; public string siteType; public LocalizedText summary;
        public string confidence; public bool playable; public string worldBuilder;
        public List<PoiDef> pois; public List<CharacterDef> characters; public List<QuestDef> quests; public List<ReferenceDef> references;
    }
    [Serializable] public class PackManifest
    {
        public string id; public string packVersion; public string minAppVersion;
        public string countryCode; public LocalizedText country; public LocalizedText civilization;
        public int yearFrom; public int yearTo; public string periodConfidence; // yearFrom negative = BCE
        public string status; // playable | planned
        public List<SiteDef> sites;
        public bool IsPlayable => status == "playable";
    }
}
