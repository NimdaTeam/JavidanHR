using _0_Framework.GenericRepositoy.Interface;
using _0_Framework.GenericRepositoy.Service;
using Microsoft.EntityFrameworkCore;
using PayrollSystem.Application.Utilities;
using PayrollSystem.Domain.Common;
using PayrollSystem.Domain.Entities.PayItem;
using PayrollSystem.Domain.Interfaces.PayItem;
using PayrollSystem.Infrastructure.Persistence.Context;

namespace PayrollSystem.Infrastructure.Repositories.PayItem
{
    public class PayItemRepository : RepositoryService<long, Domain.Entities.PayItem.PayItem>, IPayItemRepository
    {
        private readonly PayrollSystemContext _context;

        public PayItemRepository(PayrollSystemContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Domain.Entities.PayItem.PayItem?> GetBySystemCodeAsync(string systemCode, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Domain.Entities.PayItem.PayItem>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.SystemCode == systemCode, cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Domain.Entities.PayItem.PayItem>> GetActivePayItemsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<Domain.Entities.PayItem.PayItem>()
                .AsNoTracking()
                .Where(p => p.IsActive)
                .OrderBy(p => p.SortOrder)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<PayItemFormula?> GetActiveFormulaAsync(long payItemId, DateTime atDate, CancellationToken cancellationToken = default)
        {
            return await _context.Set<PayItemFormula>()
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.PayItemId == payItemId &&
                                          f.IsActive &&
                                          f.ValidFromDate <= atDate &&
                                          (!f.ValidToDate.HasValue || f.ValidToDate >= atDate),
                                          cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<List<Domain.Entities.PayItem.PayItem>> GetPayItemsByTypeAsync(Domain.Entities.PayItem.PayItem.PayItemType type, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Domain.Entities.PayItem.PayItem>()
                .AsNoTracking()
                .Where(p => p.Type == type && p.IsActive)
                .OrderBy(p => p.SortOrder)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task RefreshCacheAsync()
        {
            var activeCustomCodes = await GetAllByConditionAsync(x=>x.IsActive && x.IsCustom);
            var codes = activeCustomCodes.Select(c => new PayItemConstants.PayItemDto()
            {
                Id = c.Id,
                Name = c.Name,
                DataType = c.DataType,
                IsActive = c.IsActive,
                SystemCode = c.SystemCode,
                Formulas = c.Formulas.Select(f=> new PayItemConstants.PayItemFormulaDto()
                {
                    IsActive = f.IsActive,
                    Formula = f.Formula,
                    Id = f.Id,
                    ValidFromDate = f.ValidFromDate,
                    ValidToDate = f.ValidToDate,
                    Version = f.Version
                }).ToList(),
                IsCustom = c.IsCustom,
                IsEditable = c.IsEditable,
                IsInsured = c.IsInsured,
                IsTaxable = c.IsTaxable,
                SortOrder = c.SortOrder,
                Type = c.Type
            }).ToList();
            PayItemConstants.LoadCustomCodesFromDatabase(codes);
        }

        public async Task<bool> IsValidCodeAsync(string code)
        {
            if (PayItemConstants.IsValidCode(code))
                return true;

            // اگر در کش نبود، از دیتابیس چک کن و کش را رفرش کن
            var exists = await _context.PayItems.AnyAsync(x=>x.SystemCode == code);
            if (exists)
                await RefreshCacheAsync();

            return exists || PayItemConstants.DefaultPayItems.Any(x=>x.SystemCode ==  code?.Trim().ToUpperInvariant());
        }

        public async Task<PayItemFormula?> GetActiveFormulaAsync(long payItemId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<PayItemFormula>()
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.PayItemId == payItemId &&
                                          f.IsActive,
                    cancellationToken)
                .ConfigureAwait(false);
        }
    }
}