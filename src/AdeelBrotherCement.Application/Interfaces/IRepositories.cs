using AdeelBrotherCement.Domain.Entities;
using AdeelBrotherCement.Domain.Enums;

namespace AdeelBrotherCement.Application.Interfaces;

public interface IProductRepository
{
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product> CreateAsync(Product product, CancellationToken ct = default);
    Task<Product> UpdateAsync(Product product, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetByCategoryAsync(ProductCategory category, CancellationToken ct = default);
}

public interface ITransactionRepository
{
    Task<IReadOnlyList<SaleTransaction>> GetAllAsync(CancellationToken ct = default);
    Task<SaleTransaction?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SaleTransaction> CreateAsync(SaleTransaction transaction, CancellationToken ct = default);
    Task<SaleTransaction> UpdateAsync(SaleTransaction transaction, CancellationToken ct = default);
    Task<IReadOnlyList<SaleTransaction>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
    Task<string> GetNextSlipNumberAsync(DateTime date, CancellationToken ct = default);
}

public interface IExpenseRepository
{
    Task<IReadOnlyList<Expense>> GetAllAsync(CancellationToken ct = default);
    Task<Expense> CreateAsync(Expense expense, CancellationToken ct = default);
    Task<Expense?> UpdateAsync(Expense expense, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Expense>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken ct = default);
}

public interface ICustomerRepository
{
    Task<IReadOnlyList<Customer>> GetAllAsync(CancellationToken ct = default);
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Customer?> GetByPhoneAsync(string phone, CancellationToken ct = default);
    Task<Customer?> GetByNameAndPhoneAsync(string name, string phone, CancellationToken ct = default);
    Task<IReadOnlyList<Customer>> SearchAsync(string query, CancellationToken ct = default);
    Task<Customer> CreateAsync(Customer customer, CancellationToken ct = default);
    Task<Customer> UpdateAsync(Customer customer, CancellationToken ct = default);
}

public interface ICustomerPaymentRepository
{
    Task<IReadOnlyList<CustomerPayment>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<CustomerPayment>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default);
    Task<CustomerPayment> CreateAsync(CustomerPayment payment, CancellationToken ct = default);
}

public interface IStockRepository
{
    Task AdjustStockAsync(Guid productId, decimal quantityChange, string reason, CancellationToken ct = default);
    Task SetStockAsync(Guid productId, decimal quantity, string reason, CancellationToken ct = default);
    Task<IReadOnlyList<StockAdjustment>> GetAdjustmentsAsync(CancellationToken ct = default);
}

public interface IUserRepository
{
    Task<IReadOnlyList<AppUser>> GetAllAsync(CancellationToken ct = default);
    Task<AppUser?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AppUser?> GetByUsernameAsync(string username, CancellationToken ct = default);
    Task<AppUser> CreateAsync(AppUser user, CancellationToken ct = default);
    Task<AppUser> UpdateAsync(AppUser user, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}

public interface IDealerRepository
{
    Task<IReadOnlyList<Dealer>> GetAllAsync(CancellationToken ct = default);
    Task<Dealer?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Dealer?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<Dealer> CreateAsync(Dealer dealer, CancellationToken ct = default);
    Task<Dealer> UpdateAsync(Dealer dealer, CancellationToken ct = default);
}

public interface IDealerPurchaseRepository
{
    Task<IReadOnlyList<DealerPurchase>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<DealerPurchase>> GetByDealerIdAsync(Guid dealerId, CancellationToken ct = default);
    Task<DealerPurchase> CreateAsync(DealerPurchase purchase, CancellationToken ct = default);
    Task<DealerPurchase> UpdateAsync(DealerPurchase purchase, CancellationToken ct = default);
}

public interface IDealerPaymentRepository
{
    Task<IReadOnlyList<DealerPayment>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<DealerPayment>> GetByDealerIdAsync(Guid dealerId, CancellationToken ct = default);
    Task<DealerPayment> CreateAsync(DealerPayment payment, CancellationToken ct = default);
}

public interface IAdvanceBookingRepository
{
    Task<IReadOnlyList<AdvanceBooking>> GetAllAsync(CancellationToken ct = default);
    Task<AdvanceBooking?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<AdvanceBooking> CreateAsync(AdvanceBooking booking, CancellationToken ct = default);
    Task<AdvanceBooking> UpdateAsync(AdvanceBooking booking, CancellationToken ct = default);
}
