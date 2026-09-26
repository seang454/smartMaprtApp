using SmallMartApp.Core.Common;

namespace SmallMartApp.Core.Features.Auth;

public class User : BaseEntity
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string WorkingShift { get; set; } = "Morning";
    public UserRole Role { get; set; } = UserRole.Cashier;
    public bool IsActive { get; set; } = true;
}
