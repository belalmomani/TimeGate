using System.Collections.Generic;
using TimeGate.Data;
namespace TimeGate.Core
{
    public static class Loc
    {
        public static string Lang = "ar";
        public static bool IsRtl => Lang == "ar";

        static readonly Dictionary<string, (string ar, string en)> S = new Dictionary<string, (string, string)>
        {
            ["title"] = ("بوابة الزمن", "TimeGate"),
            ["subtitle"] = ("اعبر البوابة. استكشف التاريخ.", "Cross the gate. Explore history."),
            ["random"] = ("سفر عشوائي", "Random travel"),
            ["byCountry"] = ("اختيار الدولة", "By country"),
            ["byCiv"] = ("اختيار الحضارة", "By civilization"),
            ["history"] = ("سجل الرحلات", "Trip history"),
            ["favorites"] = ("المفضلة", "Favorites"),
            ["settings"] = ("الإعدادات", "Settings"),
            ["about"] = ("التراخيص والمصادر", "Licenses & sources"),
            ["back"] = ("رجوع", "Back"),
            ["travel"] = ("سافر", "Travel"),
            ["reroll"] = ("أعد الرمي", "Roll again"),
            ["confirm"] = ("أكّد السفر", "Confirm travel"),
            ["roll"] = ("ارمِ النرد", "Roll the dice"),
            ["rolling"] = ("...", "..."),
            ["inDev"] = ("قيد التطوير", "In development"),
            ["conf_documented"] = ("موثّق", "Documented"),
            ["conf_approximate"] = ("تقريبي / محل نقاش", "Approximate / debated"),
            ["conf_artistic"] = ("إعادة بناء فنية", "Artistic reconstruction"),
            ["references"] = ("المراجع", "References"),
            ["unverified"] = ("(الرابط لم يُتحقق منه بعد)", "(link not yet verified)"),
            ["language"] = ("اللغة: العربية ⇄ English", "Language: العربية ⇄ English"),
            ["sensitivity"] = ("حساسية الكاميرا", "Camera sensitivity"),
            ["invertY"] = ("عكس المحور الرأسي", "Invert vertical look"),
            ["reduceFx"] = ("تقليل المؤثرات", "Reduce effects"),
            ["quality"] = ("الجودة", "Quality"),
            ["interact"] = ("تفاعل", "Interact"),
            ["gate"] = ("البوابة", "Gate"),
            ["boost"] = ("سريع", "Boost"),
            ["slow"] = ("تحليق بطيء", "Slow"),
            ["camView"] = ("المنظور", "View"),
            ["camReset"] = ("إعادة الكاميرا", "Reset cam"),
            ["quest"] = ("المهمة", "Quest"),
            ["questDone"] = ("اكتملت المهمة", "Quest complete"),
            ["noVisits"] = ("لا توجد رحلات بعد.", "No trips yet."),
            ["noFavs"] = ("لا توجد مفضلة بعد.", "No favorites yet."),
            ["bundled"] = ("الحزمة مضمّنة في التطبيق (لا تحتاج تنزيلًا في هذا الإصدار).", "Pack is bundled in the app (no download needed in this version)."),
            ["noPlayable"] = ("لا توجد وجهات جاهزة.", "No playable destinations."),
            ["services"] = ("الخدمات", "Services"),
            ["notConfigured"] = ("غير مُعدّة", "Not configured"),
            ["edge"] = ("حدود المنطقة المتاحة للاستكشاف", "Edge of explorable area"),
            ["fav"] = ("إضافة للمفضلة", "Add to favorites"),
            ["unfav"] = ("إزالة من المفضلة", "Remove from favorites"),
            ["close"] = ("إغلاق", "Close"),
            ["placeholderNote"] = ("هذا العالم مبني برمجيًا من أشكال أولية (إعادة بناء فنية) بانتظار أصول مرخّصة.", "This world is procedurally built from primitives (artistic reconstruction) pending licensed assets."),
            ["country"] = ("الدولة", "Country"),
            ["civ"] = ("الحضارة", "Civilization"),
            ["period"] = ("الفترة", "Period"),
            ["sites"] = ("المواقع", "Sites"),
            ["firstPerson"] = ("منظور أول", "First person"),
            ["thirdPerson"] = ("منظور ثالث", "Third person"),
        };

        public static string T(string key) =>
            S.TryGetValue(key, out var v) ? (Lang == "ar" ? v.ar : v.en) : key;

        public static string Pick(LocalizedText t) => t == null ? "" : (Lang == "ar" ? (string.IsNullOrEmpty(t.ar) ? t.en : t.ar) : (string.IsNullOrEmpty(t.en) ? t.ar : t.en));
    }
}
