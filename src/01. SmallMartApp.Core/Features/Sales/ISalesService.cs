using SmallMartApp.Core.Common;

namespace SmallMartApp.Core.Features.Sales;

public interface ISalesService
{
    Task<Result<Sale>> ProcessSaleAsync(
        List<(int ProductId, int Quantity)> items, 
        decimal cashReceived, 
        int? shiftId = null, 
        int? customerId = null, 
        decimal discountAmount = 0m, 
        PaymentMethod paymentMethod = PaymentMethod.Cash);

    Task<List<Sale>> GetRecentSalesAsync(int count = 20);
}
