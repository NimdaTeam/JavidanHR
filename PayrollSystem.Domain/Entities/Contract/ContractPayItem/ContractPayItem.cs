using _0_Framework.EntityBase;
using Ardalis.GuardClauses;

namespace PayrollSystem.Domain.Entities.Contract.ContractPayItem
{
    public class ContractPayItem : EntityBase
    {
        public long ContractId { get; private set; }
        public long PayItemId { get; private set; }
        public decimal? Value { get; private set; } //for user input Items

        private ContractPayItem() { }

        internal ContractPayItem(long contractId, long payItemId, decimal? value = null)
        {
            Guard.Against.NegativeOrZero(contractId, nameof(contractId));
            Guard.Against.NegativeOrZero(payItemId, nameof(payItemId));

            ContractId = contractId;
            PayItemId = payItemId;
            Value = value;
        }

        // متد به‌روزرسانی مقدار (برای آیتم‌های با ورودی کاربر)
        public void UpdateValue(decimal? newValue)
        {
            // در اینجا می‌توان اعتبارسنجی بیشتری انجام داد (مثلاً بر اساس نوع آیتم)
            Value = newValue;
        }

        // بررسی اینکه آیا آیتم نیاز به مقدار ورودی کاربر دارد (در لایه بالاتر می‌توان از PayItem.DataType استفاده کرد)
        public bool RequiresUserInput => Value == null;
    }
}