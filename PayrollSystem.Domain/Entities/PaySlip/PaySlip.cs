using _0_Framework.EntityBase;
using Ardalis.GuardClauses;
using System;
using System.Collections.Generic;
using System.Linq;
using static PayrollSystem.Domain.Entities.PayItem.PayItem;

namespace PayrollSystem.Domain.Entities.PaySlip
{
    public class PaySlip : EntityBase
    {
        public long EmployeeId { get; private set; }
        public long ContractId { get; private set; }
        public int Year { get; private set; }
        public int Month { get; private set; }
        public PaySlipStatus Status { get; private set; }
        public DateTime IssueDate { get; private set; }

        // جمع‌های نهایی (برای کارایی و جلوگیری از محاسبه‌های تکراری)
        public decimal TotalEarnings { get; private set; }
        public decimal TotalDeductions { get; private set; }
        public decimal NetPay { get; private set; }

        // لیست آیتم‌های فیش (ارتباط یک به چند)
        private readonly List<PaySlipItem.PaySlipItem> _items = new();
        public IReadOnlyCollection<PaySlipItem.PaySlipItem> Items => _items.AsReadOnly();

        private PaySlip() { }

        public PaySlip(long employeeId, long contractId, int year, int month, DateTime issueDate)
        {
            Guard.Against.NegativeOrZero(employeeId, nameof(employeeId));
            Guard.Against.NegativeOrZero(contractId, nameof(contractId));
            Guard.Against.OutOfRange(year, nameof(year), 1400, 1500);
            Guard.Against.OutOfRange(month, nameof(month), 1, 12);
            Guard.Against.Null(issueDate, nameof(issueDate));

            EmployeeId = employeeId;
            ContractId = contractId;
            Year = year;
            Month = month;
            IssueDate = issueDate;
            Status = PaySlipStatus.Draft;
            TotalEarnings = 0;
            TotalDeductions = 0;
            NetPay = 0;
        }

        // ---------------------- مدیریت آیتم‌های فیش ----------------------
        public void AddItem(long payItemId, decimal calculatedValue, decimal? manualOverride = null)
        {
            if (Status == PaySlipStatus.Finalized)
                throw new InvalidOperationException("فیش نهایی شده، امکان اضافه کردن آیتم وجود ندارد");

            if (_items.Any(i => i.PayItemId == payItemId))
                throw new InvalidOperationException("این آیتم قبلاً به فیش اضافه شده است");

            var item = new PaySlipItem.PaySlipItem(Id, payItemId, calculatedValue, manualOverride);
            _items.Add(item);
            RecalculateTotals();
        }

        public void UpdateItemCalculatedValue(long payItemId, decimal newCalculatedValue)
        {
            if (Status == PaySlipStatus.Finalized)
                throw new InvalidOperationException("فیش نهایی شده، امکان تغییر وجود ندارد");

            var item = _items.FirstOrDefault(i => i.PayItemId == payItemId);
            if (item == null)
                throw new ArgumentException("آیتم مورد نظر یافت نشد");

            item.UpdateCalculatedValue(newCalculatedValue);
            RecalculateTotals();
        }

        public void OverrideItemValue(long payItemId, decimal? manualValue)
        {
            if (Status == PaySlipStatus.Finalized)
                throw new InvalidOperationException("فیش نهایی شده، امکان تغییر وجود ندارد");

            var item = _items.FirstOrDefault(i => i.PayItemId == payItemId);
            if (item == null)
                throw new ArgumentException("آیتم مورد نظر یافت نشد");

            item.OverrideValue(manualValue);
            RecalculateTotals();
        }

        public void RemoveItem(long payItemId)
        {
            if (Status == PaySlipStatus.Finalized)
                throw new InvalidOperationException("فیش نهایی شده، امکان حذف وجود ندارد");

            var item = _items.FirstOrDefault(i => i.PayItemId == payItemId);
            if (item != null)
            {
                _items.Remove(item);
                RecalculateTotals();
            }
        }

        // محاسبه مجدد جمع‌ها بر اساس نوع آیتم‌ها (نیاز به اطلاعات PayItem برای تشخیص Earning/Deduction)
        // این متد باید با دریافت نوع آیتم‌ها از یک سرویس یا با بارگذاری PayItem‌ها کار کند.
        // برای سادگی، فرض می‌کنیم یک دیکشنری خارجی نوع آیتم را مشخص می‌کند.
        // در عمل می‌توانید PayItem را از ریپازیتوری دریافت کنید یا نوع را در خود PaySlipItem ذخیره کنید.
        // در اینجا متد را با فرض دریافت تابعی برای تعیین نوع پیاده می‌کنیم:
        public void RecalculateTotals(Func<long, PayItemType> getPayItemType)
        {
            decimal earnings = 0;
            decimal deductions = 0;

            foreach (var item in _items)
            {
                var type = getPayItemType(item.PayItemId);
                if (type == PayItemType.Earning)
                    earnings += item.FinalValue;
                else if (type == PayItemType.Deduction)
                    deductions += item.FinalValue;
                // نوع Info نادیده گرفته می‌شود
            }

            TotalEarnings = earnings;
            TotalDeductions = deductions;
            NetPay = earnings - deductions;
        }

        // نسخه ساده‌تر: اگر نوع آیتم‌ها را در خود PaySlipItem ذخیره کنید (پیشنهاد نمی‌شود چون باعث تکرار اطلاعات می‌شود)
        // یا اینکه هنگام افزودن آیتم، نوع را از بیرون دریافت و در یک دیکشنری داخلی نگهداری کنید.
        // برای نمونه، یک متد کمکی داخلی که با فرض دارا بودن دیکشنری نوع‌ها کار می‌کند:
        private void RecalculateTotals()
        {
            // این متد بدون دریافت نوع قابل پیاده‌سازی نیست. برای جلوگیری از خطا، آن را overload کرده‌ایم.
            throw new InvalidOperationException("لطفاً از نسخه RecalculateTotals(Func<long, PayItemType>) استفاده کنید");
        }

        // ---------------------- تغییر وضعیت فیش ----------------------
        public void Finalize()
        {
            if (Status == PaySlipStatus.Finalized)
                throw new InvalidOperationException("فیش قبلاً نهایی شده است");
            Status = PaySlipStatus.Finalized;
        }

        public void RevertToDraft()
        {
            if (Status == PaySlipStatus.Draft)
                return;
            Status = PaySlipStatus.Draft;
        }

        // ---------------------- متدهای کمکی ----------------------
        public bool IsForDate(int year, int month) => Year == year && Month == month;
    }

    // ---------------------- Enum وضعیت فیش ----------------------
    public enum PaySlipStatus
    {
        Draft,
        Finalized
    }
}