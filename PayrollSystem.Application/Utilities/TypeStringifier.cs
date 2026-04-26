using PayrollSystem.Domain.Entities.PayItem;

namespace PayrollSystem.Application.Utilities
{
    public static class TypeStringifier
    {
        public static string PayItemTypeToString(this PayItem.PayItemType type)
        {
            return type switch
            {
                PayItem.PayItemType.Earning => "مزایا",
                PayItem.PayItemType.Deduction => "کسورات",
                PayItem.PayItemType.Info => "اطلاعاتی",
                _ => "-"
            };
        }

        public static string PayItemDataTypeToString(this PayItem.PayItemDataType type)
        {
            return type switch
            {
                PayItem.PayItemDataType.Boolean => "بله/خیر",
                PayItem.PayItemDataType.Decimal => "عدد اعشاری",
                PayItem.PayItemDataType.Formula => "فرمول",
                PayItem.PayItemDataType.UserInput => "ورودی کاربر",
                _ => "-"
            };
        }

    }
}
