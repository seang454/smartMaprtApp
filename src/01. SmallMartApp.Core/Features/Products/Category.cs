using SmallMartApp.Core.Common;

namespace SmallMartApp.Core.Features.Products;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
