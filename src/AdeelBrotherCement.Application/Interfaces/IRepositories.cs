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
