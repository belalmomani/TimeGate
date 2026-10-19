using System.Linq;
using TimeGate.Data;
namespace TimeGate.Travel
{
    public class QuestService
    {
        readonly SaveSystem save; readonly SiteDef site;
        public QuestService(SaveSystem s, SiteDef site) { save = s; this.site = site; }
        public QuestDef Active => site.quests?.FirstOrDefault(q => !save.Data.completedQuests.Contains(q.id));
        public (int done, int total) Progress(QuestDef q) =>
            (q.objectivePoiIds.Count(id => save.Data.inspectedPois.Contains(id)), q.objectivePoiIds.Count);
        /// Returns the quest completed by this inspection, if any. Rewards/completion are recorded once only.
        public QuestDef Inspect(PoiDef poi)
        {
            if (!save.Data.inspectedPois.Contains(poi.id)) save.Data.inspectedPois.Add(poi.id);
            QuestDef done = null;
            if (site.quests != null)
                foreach (var q in site.quests)
                    if (!save.Data.completedQuests.Contains(q.id) && q.objectivePoiIds.All(id => save.Data.inspectedPois.Contains(id)))
                    { save.Data.completedQuests.Add(q.id); done = q; }
            save.Save(); return done;
        }
    }
}
