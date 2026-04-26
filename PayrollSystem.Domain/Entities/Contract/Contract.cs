using _0_Framework.EntityBase;
using Ardalis.GuardClauses;

namespace PayrollSystem.Domain.Entities.Contract
{
    public class Contract : EntityBase
    {
        public long EmployeeId { get; private set; }
        public long WorkshopId { get; private set; }
        public DateTime ValidFromDate { get; private set; }
        public DateTime? ValidToDate { get; private set; }
        public bool IsActive { get; private set; }
        public ContractStatus Status { get; private set; }

        // رابطه یک به چند با آیتم‌های پرداختی قرارداد
        private readonly List<ContractPayItem.ContractPayItem> _payItems = new();
        public IReadOnlyCollection<ContractPayItem.ContractPayItem> PayItems => _payItems.AsReadOnly();

        private Contract() { }

        public Contract(long employeeId, long workshopId, DateTime validFromDate, DateTime? validToDate = null)
        {
            Guard.Against.NegativeOrZero(employeeId, nameof(employeeId));
            Guard.Against.NegativeOrZero(workshopId, nameof(workshopId));
            Guard.Against.Null(validFromDate, nameof(validFromDate));

            if (validToDate.HasValue && validToDate.Value < validFromDate)
                throw new ArgumentException("تاریخ پایان اعتبار باید بعد از تاریخ شروع باشد");

            EmployeeId = employeeId;
            WorkshopId = workshopId;
            ValidFromDate = validFromDate;
            ValidToDate = validToDate;
            IsActive = true;
            Status = ContractStatus.Active;
        }

        // ---------------------- متدهای مدیریت آیتم‌های قرارداد ----------------------
        public void AddPayItem(long payItemId, decimal? value = null)
        {
            if (_payItems.Any(p => p.PayItemId == payItemId))
                throw new InvalidOperationException("این آیتم قبلاً به قرارداد اضافه شده است");

            var payItem = new ContractPayItem.ContractPayItem(Id, payItemId, value);
            _payItems.Add(payItem);
        }

        public void UpdatePayItemValue(long payItemId, decimal? newValue)
        {
            var payItem = _payItems.FirstOrDefault(p => p.PayItemId == payItemId);
            if (payItem == null)
                throw new ArgumentException("آیتم مورد نظر در قرارداد یافت نشد");

            payItem.UpdateValue(newValue);
        }

        public void RemovePayItem(long payItemId)
        {
            var payItem = _payItems.FirstOrDefault(p => p.PayItemId == payItemId);
            if (payItem != null)
                _payItems.Remove(payItem);
        }

        // ---------------------- متدهای مدیریت وضعیت قرارداد (مطابق نسخه قبلی) ----------------------
        public void UpdateValidFromDate(DateTime newValidFromDate)
        {
            Guard.Against.Null(newValidFromDate, nameof(newValidFromDate));
            if (ValidToDate.HasValue && newValidFromDate > ValidToDate.Value)
                throw new ArgumentException("تاریخ شروع اعتبار باید قبل از تاریخ پایان باشد");
            ValidFromDate = newValidFromDate;
        }

        public void UpdateValidToDate(DateTime? newValidToDate)
        {
            if (newValidToDate.HasValue && newValidToDate.Value < ValidFromDate)
                throw new ArgumentException("تاریخ پایان اعتبار باید بعد از تاریخ شروع باشد");
            ValidToDate = newValidToDate;
            if (newValidToDate.HasValue && newValidToDate.Value < DateTime.Now.Date)
                Deactivate();
        }

        public void Activate()
        {
            if (IsActive) return;
            IsActive = true;
            Status = ContractStatus.Active;
        }

        public void Deactivate()
        {
            if (!IsActive) return;
            IsActive = false;
            Status = ContractStatus.Inactive;
        }

        public void Terminate()
        {
            IsActive = false;
            Status = ContractStatus.Terminated;
            ValidToDate = DateTime.Now.Date;
        }

        public void ChangeStatus(ContractStatus newStatus)
        {
            Status = newStatus;
            IsActive = (newStatus == ContractStatus.Active);
        }

        public bool IsValidAt(DateTime date)
        {
            return IsActive &&
                   ValidFromDate <= date &&
                   (!ValidToDate.HasValue || ValidToDate.Value >= date);
        }

        public bool IsCurrentlyValid() => IsValidAt(DateTime.Now);
    }

    public enum ContractStatus
    {
        Active,
        Inactive,
        Terminated,
        Expired
    }
}