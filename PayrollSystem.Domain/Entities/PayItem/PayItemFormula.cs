using _0_Framework.EntityBase;
using Ardalis.GuardClauses;
using PayrollSystem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace PayrollSystem.Domain.Entities.PayItem
{
    public class PayItemFormula : EntityBase
    {
        public long PayItemId { get; private set; }
        public string Formula { get; private set; }
        public long Version { get; private set; }
        public DateTime ValidFromDate { get; private set; }
        public DateTime? ValidToDate { get; private set; }
        public bool IsActive { get; private set; }

        private PayItemFormula() { }

        internal PayItemFormula(long payItemId, string formula, DateTime validFromDate, DateTime? validToDate = null)
        {
            Guard.Against.NegativeOrZero(payItemId, nameof(payItemId));
            Guard.Against.Null(validFromDate, nameof(validFromDate));

            if (validToDate.HasValue && validToDate.Value < validFromDate)
                throw new ArgumentException("تاریخ پایان اعتبار باید بعد از تاریخ شروع باشد");

            ValidateFormula(formula);

            PayItemId = payItemId;
            Formula = formula.Trim();
            Version = 1;
            ValidFromDate = validFromDate;
            ValidToDate = validToDate;
            IsActive = true;
        }

        // ---------------------- متدهای عمومی (برای استفاده توسط Aggregate Root) ----------------------
        public void UpdateFormula(string formula)
        {
            Guard.Against.NullOrWhiteSpace(formula, nameof(formula));
            ValidateFormula(formula);
            Formula = formula.Trim();
            Version++;
        }

        public void UpdateValidToDate(DateTime? validToDate)
        {
            if (validToDate.HasValue && validToDate.Value < ValidFromDate)
                throw new ArgumentException("تاریخ پایان اعتبار باید بعد از تاریخ شروع باشد");
            ValidToDate = validToDate;
        }

        public void Activate()
        {
            if (IsActive) return;
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        // بررسی می‌کند که آیا فرمول در تاریخ مشخص شده معتبر است (بر اساس تاریخ و وضعیت فعال)
        public bool IsValidAt(DateTime date)
        {
            return IsActive && ValidFromDate <= date && (!ValidToDate.HasValue || ValidToDate.Value >= date);
        }
        // ---------------------- متدهای کمکی خصوصی ----------------------
        private void ValidateFormula(string formula)
        {
            Guard.Against.NullOrWhiteSpace(formula, nameof(formula));
            Guard.Against.OutOfRange(formula.Length, nameof(formula), 1, 500);

            // حذف براکت‌ها و آکولادها برای اعتبارسنجی ساده‌تر (اما در خود فرمول نگهداری می‌شوند)
            string cleanFormula = formula
                .Replace("[", "")
                .Replace("]", "")
                .Replace("{", "")
                .Replace("}", "");

            var variables = ExtractVariables(cleanFormula);
            if (variables.Count == 0)
                throw new ArgumentException("فرمول باید حداقل شامل یک متغیر باشد");

            var invalidVariables = variables
                .Where(v => !PayItemConstants.IsValidCode(v))
                .ToList();
            if (invalidVariables.Any())
                throw new ArgumentException(
                    $"متغیرهای نامعتبر در فرمول: {string.Join(", ", invalidVariables)}. " +
                    $"متغیرهای مجاز: {string.Join(", ", PayItemConstants.GetAllValidCodes())}");

            ValidateFormulaSyntax(cleanFormula, variables);
        }

        private static HashSet<string> ExtractVariables(string formulaWithoutBraces)
        {
            var variables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var pattern = @"\b([A-Z_][A-Z0-9_]*)\b";
            var matches = Regex.Matches(formulaWithoutBraces, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
                variables.Add(match.Groups[1].Value.ToUpperInvariant());
            return variables;
        }

        private static void ValidateFormulaSyntax(string cleanFormula, HashSet<string> variables)
        {
            try
            {
                // IDictionary<string, decimal> به جای object —
                // تا عملگرهای ریاضی مثل * و / با اعداد صحیح سازگار باشند
                var parameter = Expression.Parameter(typeof(IDictionary<string, decimal>), "variables");

                var transformedFormula = cleanFormula;
                foreach (var varName in variables)
                    transformedFormula = Regex.Replace(
                        transformedFormula,
                        $@"\b{Regex.Escape(varName)}\b",
                        $"variables[\"{varName}\"]",
                        RegexOptions.IgnoreCase);

                var parsedExpression = DynamicExpressionParser.ParseLambda(
                    new[] { parameter },
                    typeof(decimal),
                    transformedFormula);

                // مقدار آزمایشی ۱ به عنوان decimal — بدون boxing به object
                var testDict = PayItemConstants.GetAllValidCodes()
                    .ToDictionary(code => code, _ => 1m);

                var compiled = parsedExpression.Compile();
                var result = compiled.DynamicInvoke(testDict);

                if (result == null)
                    throw new ArgumentException("فرمول مقدار null برمی‌گرداند");
            }
            catch (Exception ex) when (ex is not ArgumentException)
            {
                throw new ArgumentException($"ساختار فرمول نامعتبر است: {ex.Message}");
            }
        }

        public HashSet<string> GetFormulaVariables() => ExtractVariables(Formula.Replace("[", "").Replace("]", "").Replace("{", "").Replace("}", ""));
    }
}