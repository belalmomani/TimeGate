using System;
using System.Collections.Generic;
using UnityEngine;
using TimeGate.Data;
namespace TimeGate.World
{
    public class PoiRuntime { public PoiDef def; public Vector3 pos; }
    public class WorldResult
    {
        public GameObject root; public Vector3 spawn; public float spawnYaw; public Bounds bounds;
        public List<PoiRuntime> pois = new List<PoiRuntime>();
    }
    public static class WorldFactory
    {
        static readonly Dictionary<string, Func<SiteDef, WorldResult>> map = new Dictionary<string, Func<SiteDef, WorldResult>>
        {
            ["petra_v1"] = PetraWorld.Build,
        };
        public static bool Has(string id) => !string.IsNullOrEmpty(id) && map.ContainsKey(id);
        public static WorldResult Build(string id, SiteDef site) => map[id](site);
    }
}
