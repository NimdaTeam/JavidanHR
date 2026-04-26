using System.Collections.Concurrent;
using PayrollSystem.Domain.Entities.PayItem;

namespace PayrollSystem.Domain.Common
{
    public static class PayItemConstants
    {
        public class PayItemDto
        {
            public long? Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string SystemCode { get; set; } = string.Empty;
            public PayItem.PayItemType Type { get; set; }
            public PayItem.PayItemDataType DataType { get; set; }
            public bool IsInsured { get; set; }
            public bool IsTaxable { get; set; }
            public bool IsEditable { get; set; }
            public bool IsActive { get; set; }
            public int SortOrder { get; set; }
            public bool IsCustom { get; set; }
            // گروه‌بندی برای نمایش در فرمول‌ساز
            public string Group { get; set; } = string.Empty;
            public List<PayItemFormulaDto> Formulas { get; set; } = new();
        }

        public class PayItemFormulaDto
        {
            public long? Id { get; set; }
            public string Formula { get; set; } = string.Empty;
            public long Version { get; set; }
            public DateTime? ValidFromDate { get; set; }
            public DateTime? ValidToDate { get; set; }
            public bool IsActive { get; set; }
        }

        private const string EarningsGroupName = "مزایا";
        private const string WorkingGroupName = "کارکرد";
        private const string DeductionsGroupName = "کسورات";
        private const string InformationGroupName = "اطلاعاتی";

        // =============================================================
        // عوامل کارکردی (از سیستم حضور و غیاب - فقط خواندنی)
        // =============================================================
        public static readonly List<PayItemDto> AttendanceItems =
        [
            CreateItem("ATT_WORK_DAYS",     "روزهای کاری",              PayItem.PayItemType.Info, PayItem.PayItemDataType.Decimal, false, false, false, 0,  WorkingGroupName),
            CreateItem("ATT_WORK_HOURS",    "ساعت کارکرد",              PayItem.PayItemType.Info, PayItem.PayItemDataType.Decimal, false, false, false, 1,  WorkingGroupName),
            CreateItem("ATT_OVERTIME",      "ساعت اضافه‌کاری",          PayItem.PayItemType.Info, PayItem.PayItemDataType.Decimal, false, false, false, 2,  WorkingGroupName),
            CreateItem("ATT_SHORTAGE",      "ساعت کسری کار",            PayItem.PayItemType.Info, PayItem.PayItemDataType.Decimal, false, false, false, 3,  WorkingGroupName),
            CreateItem("ATT_ANNUAL_LEAVE",  "مرخصی استحقاقی (روز)",    PayItem.PayItemType.Info, PayItem.PayItemDataType.Decimal, false, false, false, 4,  WorkingGroupName),
            CreateItem("ATT_SICK_LEAVE",    "مرخصی استعلاجی (روز)",    PayItem.PayItemType.Info, PayItem.PayItemDataType.Decimal, false, false, false, 5,  WorkingGroupName),
            CreateItem("ATT_MISSION_HOURS", "ساعت مأموریت",             PayItem.PayItemType.Info, PayItem.PayItemDataType.Decimal, false, false, false, 6,  WorkingGroupName),
            CreateItem("ATT_HOLIDAY_WORK",  "ساعت کار در تعطیلات",     PayItem.PayItemType.Info, PayItem.PayItemDataType.Decimal, false, false, false, 7,  WorkingGroupName),
            CreateItem("ATT_NIGHT_HOURS",   "ساعت کار شبانه",          PayItem.PayItemType.Info, PayItem.PayItemDataType.Decimal, false, false, false, 8,  WorkingGroupName),
            CreateItem("ATT_ABSENCE_DAYS",  "روزهای غیبت",             PayItem.PayItemType.Info, PayItem.PayItemDataType.Decimal, false, false, false, 9,  WorkingGroupName),
        ];

        // =============================================================
        // عوامل حقوقی پایه (سیستمی)
        // =============================================================
        public static readonly List<PayItemDto> DefaultPayItems =
        [
            // ==================== مزایا ====================
            CreateItem("PAY_BASE",          "حقوق پایه",                PayItem.PayItemType.Earning, PayItem.PayItemDataType.Decimal, true,  true,  true, 10, "مزایا",
                [new PayItemFormulaDto { Formula = "ATT_WORK_DAYS * PAY_DAILY_RATE", Version = 1, IsActive = true }]),
            CreateItem("PAY_DAILY_RATE",    "نرخ روزانه",               PayItem.PayItemType.Earning, PayItem.PayItemDataType.UserInput, false, false, true, 11, EarningsGroupName),
            CreateItem("PAY_CHILD",         "حق اولاد",                 PayItem.PayItemType.Earning, PayItem.PayItemDataType.Decimal, false, true,  true, 12, EarningsGroupName),
            CreateItem("PAY_HOUSING",       "حق مسکن",                  PayItem.PayItemType.Earning, PayItem.PayItemDataType.Decimal, false, true,  true, 13, EarningsGroupName),
            CreateItem("PAY_FOOD",          "حق بن",                    PayItem.PayItemType.Earning, PayItem.PayItemDataType.Decimal, false, true,  true, 14, EarningsGroupName),
            CreateItem("PAY_MARRIAGE",      "حق تأهل",                  PayItem.PayItemType.Earning, PayItem.PayItemDataType.Decimal, false, true,  true, 15, EarningsGroupName),
            CreateItem("PAY_OVERTIME",      "اضافه‌کاری",               PayItem.PayItemType.Earning, PayItem.PayItemDataType.Formula,       true,  true,  true, 16, EarningsGroupName),
            CreateItem("PAY_MISSION",       "مأموریت",                   PayItem.PayItemType.Earning, PayItem.PayItemDataType.Decimal, true,  true,  true, 17, EarningsGroupName),
            CreateItem("PAY_SENIORITY",     "پایه سنوات",               PayItem.PayItemType.Earning, PayItem.PayItemDataType.Decimal, true,  true,  true, 18, EarningsGroupName),
            CreateItem("PAY_SKILL",         "حق مهارت",                 PayItem.PayItemType.Earning, PayItem.PayItemDataType.Decimal, false, true,  true, 19, EarningsGroupName),
            CreateItem("PAY_RESPONSIBILITY","حق مسئولیت",               PayItem.PayItemType.Earning, PayItem.PayItemDataType.Decimal, true,  true,  true, 20, EarningsGroupName),
            CreateItem("PAY_EDUCATION",     "حق تحصیلات",               PayItem.PayItemType.Earning, PayItem.PayItemDataType.Decimal, false, true,  true, 21, EarningsGroupName),
            CreateItem("PAY_EXTRA",         "فوق‌العاده",                PayItem.PayItemType.Earning, PayItem.PayItemDataType.Decimal, true,  true,  true, 22, EarningsGroupName),
            CreateItem("PAY_BONUS",         "پاداش",                    PayItem.PayItemType.Earning, PayItem.PayItemDataType.Decimal, true,  true,  true, 23, EarningsGroupName),
            CreateItem("PAY_ATTRACT",       "حق جذب",                   PayItem.PayItemType.Earning, PayItem.PayItemDataType.Decimal, false, true,  true, 24, EarningsGroupName),
            CreateItem("PAY_SHIFT",         "حق شیفت",                  PayItem.PayItemType.Earning, PayItem.PayItemDataType.Decimal, true,  true,  true, 25, EarningsGroupName),
            CreateItem("PAY_HARDSHIP",      "سختی کار",                 PayItem.PayItemType.Earning, PayItem.PayItemDataType.Decimal, true,  true,  true, 26, EarningsGroupName),
            CreateItem("PAY_MEAL",          "حق غذا",                   PayItem.PayItemType.Earning, PayItem.PayItemDataType.Decimal, false, true,  true, 27, EarningsGroupName),
            CreateItem("PAY_CASHIER",       "حق صندوقداری",             PayItem.PayItemType.Earning, PayItem.PayItemDataType.Decimal, false, true,  true, 28, EarningsGroupName),
            CreateItem("PAY_TRANSPORT",     "ایاب و ذهاب",              PayItem.PayItemType.Earning, PayItem.PayItemDataType.Decimal, false, true,  true, 29, EarningsGroupName),
            CreateItem("PAY_EID",           "عیدی",                     PayItem.PayItemType.Earning, PayItem.PayItemDataType.Decimal, true,  true,  true, 30, EarningsGroupName),
            CreateItem("PAY_OTHER_EARN",    "سایر مزایا",               PayItem.PayItemType.Earning, PayItem.PayItemDataType.Decimal, true,  true,  true, 31, EarningsGroupName),
            CreateItem("PAY_YEAR_BONUS",    "سنوات پایان سال",          PayItem.PayItemType.Earning, PayItem.PayItemDataType.Decimal, true,  true,  true, 32, EarningsGroupName),

            // ==================== کسورات ====================
            CreateItem("DED_SHORTAGE",      "کسر کارکرد",               PayItem.PayItemType.Deduction , PayItem.PayItemDataType.Formula,      false, false, true, 40, DeductionsGroupName),
            CreateItem("DED_ADVANCE",       "مساعده",                   PayItem.PayItemType.Deduction, PayItem.PayItemDataType.Decimal, false, false, true, 41, DeductionsGroupName),
            CreateItem("DED_LOAN",          "تسهیلات/وام",              PayItem.PayItemType.Deduction, PayItem.PayItemDataType.Decimal, false, false, true, 42, DeductionsGroupName),
            CreateItem("DED_INSURANCE_EMP", "بیمه سهم کارمند",         PayItem.PayItemType.Deduction, PayItem.PayItemDataType.Formula,      false, false, false,43, DeductionsGroupName),
            CreateItem("DED_TAX",           "مالیات حقوق",              PayItem.PayItemType.Deduction, PayItem.PayItemDataType.Formula,      false, false, false,44, DeductionsGroupName),
            CreateItem("DED_OTHER",         "سایر کسورات",              PayItem.PayItemType.Deduction, PayItem.PayItemDataType.Decimal, false, false, true, 45, DeductionsGroupName),

            // ==================== اطلاعاتی ====================
            CreateItem("INF_GROSS",         "درآمد ناخالص",             PayItem.PayItemType.Info, PayItem.PayItemDataType.Formula,    false, false, false,50, InformationGroupName),
            CreateItem("INF_NET",           "درآمد خالص",               PayItem.PayItemType.Info, PayItem.PayItemDataType.Formula,    false, false, false,51, InformationGroupName),
            CreateItem("INF_INSURANCE_BASE","مبنای بیمه",               PayItem.PayItemType.Info, PayItem.PayItemDataType.Formula,    false, false, false,52, InformationGroupName),
            CreateItem("INF_TAX_BASE",      "مبنای مالیات",             PayItem.PayItemType.Info, PayItem.PayItemDataType.Formula,    false, false, false,53, InformationGroupName),
        ];

        // ترکیب هر دو گروه (برای استفاده در ولیدیشن فرمول)
        public static IEnumerable<PayItemDto> AllSystemItems =>
            AttendanceItems.Concat(DefaultPayItems);

        // ---------- کش کدهای سفارشی ----------
        private static readonly ConcurrentDictionary<string, PayItemDto> CustomPayItems =
            new(StringComparer.OrdinalIgnoreCase);

        // ---------- Factory ----------
        private static PayItemDto CreateItem(
            string systemCode, string name, PayItem.PayItemType type, PayItem.PayItemDataType dataType,
            bool isInsured, bool isTaxable, bool isEditable,
            int sortOrder, string group,
            List<PayItemFormulaDto>? formulas = null)
        {
            return new PayItemDto
            {
                Id = null,
                SystemCode = systemCode,
                Name = name,
                Type = type,
                DataType = dataType,
                IsInsured = isInsured,
                IsTaxable = isTaxable,
                IsEditable = isEditable,
                IsActive = true,
                IsCustom = false,
                SortOrder = sortOrder,
                Group = group,
                Formulas = formulas ?? new()
            };
        }

        // ---------- متدهای عمومی ----------
        public static IEnumerable<PayItemDto> GetAllValidItems()
            => AllSystemItems.Concat(CustomPayItems.Values);

        public static bool IsValidCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            code = code.Trim();


            return AllSystemItems.Any(x => x.SystemCode.Equals(code, StringComparison.OrdinalIgnoreCase))
                   || CustomPayItems.ContainsKey(code);
        }

        public static bool IsCodeExist(string code, long? id = null)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;
            code = code.Trim();

            if (id != null)
            {
                return AllSystemItems.Any(x => x.SystemCode.Equals(code, StringComparison.OrdinalIgnoreCase) && x.Id != id);
            }

            return AllSystemItems.Any(x => x.SystemCode.Equals(code, StringComparison.OrdinalIgnoreCase))
                   || CustomPayItems.ContainsKey(code);
        }

        // این متد نام قدیمی را نگه می‌داریم تا کدهای قدیمی خراب نشوند
        public static IEnumerable<string> GetAllValidCodes()
            => GetAllValidItems().Select(x => x.SystemCode);

        public static string? GetName(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            return GetAllValidItems()
                .FirstOrDefault(x => x.SystemCode.Equals(code.Trim(), StringComparison.OrdinalIgnoreCase))
                ?.Name;
        }

        public static void AddCustomCode(PayItemDto item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (string.IsNullOrWhiteSpace(item.SystemCode))
                throw new ArgumentException("کد نمی‌تواند خالی باشد");

            item.SystemCode = item.SystemCode.Trim();

            if (AllSystemItems.Any(d => d.SystemCode.Equals(item.SystemCode, StringComparison.OrdinalIgnoreCase)))
                throw new ArgumentException($"کد '{item.SystemCode}' از قبل در کدهای سیستمی وجود دارد.");

            if (CustomPayItems.ContainsKey(item.SystemCode))
                throw new ArgumentException($"کد '{item.SystemCode}' قبلاً اضافه شده است.");

            item.IsCustom = true;
            CustomPayItems.TryAdd(item.SystemCode, item);
        }

        public static void AddCustomCode(string code, string name)
        {
            AddCustomCode(new PayItemDto
            {
                SystemCode = code.Trim(),
                Name = name,
                Type = PayItem.PayItemType.Earning,
                DataType = PayItem.PayItemDataType.Decimal,
                IsInsured = false,
                IsTaxable = false,
                IsEditable = true,
                IsActive = true,
                IsCustom = true,
                SortOrder = 999,
                Group = "سفارشی"
            });
        }

        public static void AddCustomCodes(IEnumerable<PayItemDto>? codes)
        {
            if (codes == null) return;
            foreach (var code in codes) AddCustomCode(code);
        }

        public static void ClearCustomCodesCache() => CustomPayItems.Clear();

        public static void LoadCustomCodesFromDatabase(IEnumerable<PayItemDto>? customCodes)
        {
            ClearCustomCodesCache();
            AddCustomCodes(customCodes);
        }
    }
}