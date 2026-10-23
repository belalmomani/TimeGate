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
            ["account"] = ("الحساب والمزامنة", "Account & sync"),
            ["email"] = ("البريد الإلكتروني", "Email"), ["password"] = ("كلمة المرور", "Password"),
            ["signIn"] = ("تسجيل الدخول", "Sign in"), ["signUp"] = ("إنشاء حساب", "Create account"), ["signOut"] = ("تسجيل الخروج", "Sign out"),
            ["syncNow"] = ("مزامنة الآن", "Sync now"), ["deleteAcc"] = ("حذف الحساب والبيانات السحابية", "Delete account & cloud data"),
            ["cloudOff"] = ("المزامنة غير مفعّلة في هذا الإصدار. اللعب المحلي يعمل بدون حساب.", "Sync is not enabled in this build. Local play works without an account."),
            ["working"] = ("جارٍ التنفيذ...", "Working..."), ["ok"] = ("تم بنجاح", "Done"), ["confirmDel"] = ("اضغط مرة أخرى للتأكيد: سيُحذف الحساب وبياناته السحابية نهائيًا.", "Press again to confirm: the account and its cloud data will be permanently deleted."),
            ["updates"] = ("فحص تحديثات المحتوى", "Check content updates"),
            ["consentTitle"] = ("الإعلانات والخصوصية", "Ads & privacy"),
            ["consentBody"] = ("يعرض التطبيق إعلانات قليلة في البوابة فقط، ولا تظهر أثناء الطيران أو الحوار. هل توافق على عرضها؟ يمكنك الرفض ومواصلة اللعب.", "The app shows few ads, only at the gate, never during flight or dialogue. Do you agree to see them? You can decline and keep playing."),
            ["accept"] = ("أوافق", "I agree"), ["decline"] = ("لا أوافق", "No thanks"),
            ["byPeriod"] = ("اختيار الفترة / السنة", "By period / year"),
            ["achievements"] = ("الإنجازات", "Achievements"),
            ["year"] = ("السنة", "Year"), ["volume"] = ("مستوى الصوت", "Volume"),
            ["yearNote"] = ("لا توجد سنة صفر. الفترات تقريبية ما لم يُذكر خلاف ذلك.", "There is no year 0. Periods are approximate unless stated."),
            ["noSitesYear"] = ("لا توجد مواقع في هذه السنة بعد.", "No sites for this year yet."),
            ["region"] = ("المنطقة", "Area"),
            ["periodUnknown"] = ("غير محددة بعد", "Not yet specified"),
            ["fictional"] = ("شخصية تخيلية", "fictional character"),
            ["aiTag"] = ("ردّ مولَّد بالذكاء الاصطناعي — إعادة بناء وليس اقتباسًا تاريخيًا", "AI-generated — a reconstruction, not a historical quote"),
            ["send"] = ("إرسال", "Send"),
            ["typeHere"] = ("اكتب سؤالك...", "Type your question..."),
            ["you"] = ("أنت", "You"),
            ["chatNote"] = ("هذا حوار تخييلي مبني على معلومات تاريخية عامة، وليس كلام شخص حقيقي.", "This is a fictional dialogue based on general historical information, not the words of a real person."),
            ["offlineAi"] = ("الذكاء الاصطناعي غير متاح الآن؛ جرّب أحد الأسئلة المقترحة.", "AI is unavailable right now; try one of the suggested questions."),
            ["thinking"] = ("...", "..."),
            ["firstPerson"] = ("منظور أول", "First person"),
            ["thirdPerson"] = ("منظور ثالث", "Third person"),
        };

        public static string T(string key) =>
            S.TryGetValue(key, out var v) ? (Lang == "ar" ? v.ar : v.en) : key;

        public static string Pick(LocalizedText t) => t == null ? "" : (Lang == "ar" ? (string.IsNullOrEmpty(t.ar) ? t.en : t.ar) : (string.IsNullOrEmpty(t.en) ? t.ar : t.en));
    }
}
