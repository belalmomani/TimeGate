using System.Linq;
using System.Text;
using TimeGate.Core;
using TimeGate.Data;
namespace TimeGate.Dialogue
{
    /// Offline, pre-written dialogue. Answers come only from reviewed pack data — never invented.
    public class DialogueService
    {
        public static string Normalize(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            var sb = new StringBuilder();
            foreach (char ch in s.ToLowerInvariant())
            {
                if ((ch >= '\u064B' && ch <= '\u065F') || ch == '\u0640') continue;
                char c = ch;
                if (c == 'أ' || c == 'إ' || c == 'آ') c = 'ا'; else if (c == 'ة') c = 'ه'; else if (c == 'ى') c = 'ي';
                sb.Append(c);
            }
            return sb.ToString();
        }

        public (TopicDef topic, string text) Answer(CharacterDef c, string question)
        {
            var q = Normalize(question); TopicDef best = null; int bestScore = 0;
            foreach (var t in c.topics)
            {
                int score = t.keywords.Count(k => !string.IsNullOrEmpty(k) && q.Contains(Normalize(k)));
                if (Normalize(Loc.Pick(t.question)) == q) score += 10;
                if (score > bestScore) { bestScore = score; best = t; }
            }
            return best != null ? (best, Loc.Pick(best.answer)) : (null, Loc.Pick(c.fallback));
        }
    }
}
