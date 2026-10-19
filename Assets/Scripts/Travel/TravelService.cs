using System.Linq;
using TimeGate.Data;
namespace TimeGate.Travel
{
    public class TravelService
    {
        readonly DestinationRegistry reg;
        readonly System.Random rng = new System.Random();
        public TravelService(DestinationRegistry r) { reg = r; }

        /// Random travel only ever returns destinations that are genuinely playable.
        public (PackManifest pack, SiteDef site)? RollRandom()
        {
            var list = reg.Available().ToList();
            if (list.Count == 0) return null;
            return list[rng.Next(list.Count)];
        }
    }
}
