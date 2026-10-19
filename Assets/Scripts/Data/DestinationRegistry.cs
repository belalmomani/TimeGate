using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TimeGate.World;
namespace TimeGate.Data
{
    public class DestinationRegistry
    {
        public readonly List<PackManifest> Packs = new List<PackManifest>();

        public void Load()
        {
            Packs.Clear();
            foreach (var ta in Resources.LoadAll<TextAsset>("Packs"))
            {
                try
                {
                    var p = JsonUtility.FromJson<PackManifest>(ta.text);
                    if (p != null && !string.IsNullOrEmpty(p.id)) Packs.Add(p);
                    else Debug.LogWarning("[TimeGate] Invalid pack: " + ta.name);
                }
                catch (System.Exception e) { Debug.LogWarning("[TimeGate] Pack parse failed " + ta.name + ": " + e.Message); }
            }
        }

        /// A site is truly available only if its pack is playable, the site is flagged playable,
        /// and a world builder with that id is registered in code.
        public static bool IsAvailable(PackManifest p, SiteDef s) =>
            p.IsPlayable && s.playable && WorldFactory.Has(s.worldBuilder);

        public IEnumerable<(PackManifest pack, SiteDef site)> AllSites() =>
            Packs.SelectMany(p => (p.sites ?? new List<SiteDef>()).Select(s => (p, s)));

        public IEnumerable<(PackManifest pack, SiteDef site)> Available() =>
            AllSites().Where(t => IsAvailable(t.pack, t.site));

        public SiteDef FindSite(string siteId) => AllSites().Select(t => t.site).FirstOrDefault(s => s.id == siteId);
        public PackManifest FindPackOfSite(string siteId) => AllSites().Where(t => t.site.id == siteId).Select(t => t.pack).FirstOrDefault();
    }
}
