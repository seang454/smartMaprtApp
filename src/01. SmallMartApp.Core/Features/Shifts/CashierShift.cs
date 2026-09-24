using SmallMartApp.Core.Common;
using SmallMartApp.Core.Features.Auth;
using SmallMartApp.Core.Features.Sales;

namespace SmallMartApp.Core.Features.Shifts;

public class CashierShift : BaseEntity
{
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime? EndTime { get; set; }
    public decimal StartingCash { get; set; }
    public decimal ExpectedCash { get; set; }
    public decimal? ActualCash { get; set; }
    public ShiftStatus Status { get; set; } = ShiftStatus.Open;
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
