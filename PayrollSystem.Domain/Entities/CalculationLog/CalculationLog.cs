using System;
using Ardalis.GuardClauses;
using _0_Framework.EntityBase;

namespace PayrollSystem.Domain.Entities.CalculationLog
{
    public class CalculationLog : EntityBase
    {
        public long PaySlipId { get; private set; }
        public long PayItemId { get; private set; }
        public string InputValuesJson { get; private set; }
        public string FormulaUsed { get; private set; }
        public decimal ResultValue { get; private set; }
        public DateTime CalculatedAt { get; private set; } // زمان محاسبه

        private CalculationLog() { }

        public CalculationLog(
            long paySlipId,
            long payItemId,
            string inputValuesJson,
            string formulaUsed,
            decimal resultValue)
        {
            Guard.Against.NegativeOrZero(paySlipId, nameof(paySlipId));
            Guard.Against.NegativeOrZero(payItemId, nameof(payItemId));
            Guard.Against.NullOrWhiteSpace(inputValuesJson, nameof(inputValuesJson));
            Guard.Against.NullOrWhiteSpace(formulaUsed, nameof(formulaUsed));
            // بررسی مختصر فرمت JSON (اختیاری)
            if (!inputValuesJson.Trim().StartsWith("{") && !inputValuesJson.Trim().StartsWith("["))
                throw new ArgumentException("InputValuesJson باید یک JSON معتبر باشد");

            PaySlipId = paySlipId;
            PayItemId = payItemId;
            InputValuesJson = inputValuesJson.Trim();
            FormulaUsed = formulaUsed.Trim();
            ResultValue = resultValue;
            CalculatedAt = DateTime.Now;
        }

        // در صورت نیاز به روز رسانی (معمولاً لاگ‌ها تغییر نمی‌کنند، ولی برای انعطاف)
        public void UpdateResult(decimal newResult)
        {
            ResultValue = newResult;
        }
    }
}