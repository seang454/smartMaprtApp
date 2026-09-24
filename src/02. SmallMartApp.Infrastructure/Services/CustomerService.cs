using Microsoft.EntityFrameworkCore;
using SmallMartApp.Core.Common;
using SmallMartApp.Core.Features.Customers;
using SmallMartApp.Infrastructure.Persistence;

namespace SmallMartApp.Infrastructure.Services;

public class CustomerService : ICustomerService
{
    private readonly SmallMartDbContext _context;

    public CustomerService(SmallMartDbContext context)
    {
        _context = context;
    }

    public async Task<List<Customer>> GetAllAsync()
    {
        return await _context.Customers
            .OrderByDescending(c => c.Points)
            .ToListAsync();
    }

    public async Task<Customer?> GetByPhoneAsync(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;
        return await _context.Customers
            .FirstOrDefaultAsync(c => c.PhoneNumber == phone.Trim());
    }

    public async Task<Result<Customer>> AddOrUpdateAsync(Customer customer)
    {
        if (string.IsNullOrWhiteSpace(customer.FullName))
            return Result<Customer>.Failure("Customer name is required.");
        if (string.IsNullOrWhiteSpace(customer.PhoneNumber))
            return Result<Customer>.Failure("Phone number is required.");

        var existing = await _context.Customers
            .FirstOrDefaultAsync(c => c.PhoneNumber == customer.PhoneNumber && c.Id != customer.Id);
        if (existing != null)
            return Result<Customer>.Failure("A customer with this phone number already exists.");

        if (customer.Id == 0)
            await _context.Customers.AddAsync(customer);
        else
            _context.Customers.Update(customer);

        await _context.SaveChangesAsync();
        return Result<Customer>.Success(customer);
    }

    public async Task<Result> AddPointsAsync(int customerId, int pointsEarned)
    {
        var customer = await _context.Customers.FindAsync(customerId);
        if (customer == null) return Result.Failure("Customer not found.");

        customer.Points += pointsEarned;
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return Result.Failure("Customer not found.");

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return Result.Success();
    }
}
