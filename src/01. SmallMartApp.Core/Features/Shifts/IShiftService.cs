using SmallMartApp.Core.Common;

namespace SmallMartApp.Core.Features.Shifts;

public interface IShiftService
{
    Task<CashierShift?> GetCurrentActiveShiftAsync(int userId);
    Task<Result<CashierShift>> OpenShiftAsync(int userId, decimal startingCash);
    Task<Result<CashierShift>> CloseShiftAsync(int shiftId, decimal actualCash);
    Task<List<CashierShift>> GetRecentShiftsAsync(int count = 100);
}
