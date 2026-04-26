using Ardalis.GuardClauses;
using _0_Framework.EntityBase;

namespace PayrollSystem.Domain.Entities.PaySlip.PaySlipItem
{
    public class PaySlipItem : EntityBase
    {
        public long PaySlipId { get; private set; }
        public long PayItemId { get; private set; }
        public decimal CalculatedValue { get; private set; }
        public decimal? ManualOverrideValue { get; private set; }
        public decimal FinalValue { get; private set; }

        private PaySlipItem() { }

        internal PaySlipItem(long paySlipId, long payItemId, decimal calculatedValue, decimal? manualOverrideValue = null)
        {
            Guard.Against.NegativeOrZero(paySlipId, nameof(paySlipId));
            Guard.Against.NegativeOrZero(payItemId, nameof(payItemId));

            PaySlipId = paySlipId;
            PayItemId = payItemId;
            CalculatedValue = calculatedValue;
            ManualOverrideValue = manualOverrideValue;
            UpdateFinalValue();
        }

        // به‌روزرسانی مقدار محاسبه‌شده (مثلاً پس از تغییر فرمول)
        internal void UpdateCalculatedValue(decimal newCalculatedValue)
        {
            CalculatedValue = newCalculatedValue;
            UpdateFinalValue();
        }

        // اعمال مقدار دستی توسط کاربر (Override)
        public void OverrideValue(decimal? newManualValue)
        {
            ManualOverrideValue = newManualValue;
            UpdateFinalValue();
        }

        // محاسبه مقدار نهایی: در صورت وجود override از آن استفاده کن، در غیر این صورت همان calculated value
        private void UpdateFinalValue()
        {
            FinalValue = ManualOverrideValue ?? CalculatedValue;
        }
    }
}