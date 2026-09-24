using SmallMartApp.Core.Common;

namespace SmallMartApp.Core.Features.Customers;

public interface ICustomerService
{
    Task<List<Customer>> GetAllAsync();
    Task<Customer?> GetByPhoneAsync(string phone);
    Task<Result<Customer>> AddOrUpdateAsync(Customer customer);
    Task<Result> AddPointsAsync(int customerId, int pointsEarned);
    Task<Result> DeleteAsync(int id);
}
