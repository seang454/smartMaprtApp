using Microsoft.EntityFrameworkCore;
using SmallMartApp.Core.Common;
using SmallMartApp.Core.Features.Sales;
using SmallMartApp.Core.Features.Shifts;
using SmallMartApp.Infrastructure.Persistence;

namespace SmallMartApp.Infrastructure.Services;

public class ShiftService : IShiftService
{
    private readonly SmallMartDbContext _context;

    public ShiftService(SmallMartDbContext context)
    {
        _context = context;
    }

    public async Task<CashierShift?> GetCurrentActiveShiftAsync(int userId)
    {
        return await _context.CashierShifts
            .Include(s => s.User)
            .Include(s => s.Sales)
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == ShiftStatus.Open);
    }

    public async Task<Result<CashierShift>> OpenShiftAsync(int userId, decimal startingCash)
    {
        var existing = await _context.CashierShifts
            .FirstOrDefaultAsync(s => s.UserId == userId && s.Status == ShiftStatus.Open);

        if (existing != null)
            return Result<CashierShift>.Failure("You already have an active open shift.");

        var shift = new CashierShift
        {
            UserId = userId,
            StartTime = DateTime.UtcNow,
            StartingCash = startingCash,
            ExpectedCash = startingCash,
            Status = ShiftStatus.Open
        };

        await _context.CashierShifts.AddAsync(shift);
        await _context.SaveChangesAsync();
        return Result<CashierShift>.Success(shift);
    }

    public async Task<Result<CashierShift>> CloseShiftAsync(int shiftId, decimal actualCash)
    {
        var shift = await _context.CashierShifts
            .Include(s => s.Sales)
            .FirstOrDefaultAsync(s => s.Id == shiftId);

        if (shift == null) return Result<CashierShift>.Failure("Shift not found.");
        if (shift.Status == ShiftStatus.Closed) return Result<CashierShift>.Failure("Shift is already closed.");

        // Calculate total cash sales made during this shift
        decimal totalCashSales = shift.Sales
            .Where(s => s.PaymentMethod == PaymentMethod.Cash)
            .Sum(s => s.TotalAmount);

        shift.ExpectedCash = shift.StartingCash + totalCashSales;
        shift.ActualCash = actualCash;
        shift.EndTime = DateTime.UtcNow;
        shift.Status = ShiftStatus.Closed;

        await _context.SaveChangesAsync();
        return Result<CashierShift>.Success(shift);
    }

    public async Task<List<CashierShift>> GetRecentShiftsAsync(int count = 100)
    {
        return await _context.CashierShifts
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Sales)
            .OrderByDescending(s => s.StartTime)
            .Take(count)
            .ToListAsync();
    }
}
