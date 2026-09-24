using SmallMartApp.Core.Common;

namespace SmallMartApp.Core.Features.Customers;

public class Customer : BaseEntity
{
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int Points { get; set; } = 0;
}
