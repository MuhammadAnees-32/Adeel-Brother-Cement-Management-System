using AdeelBrotherCement.Application.DTOs;
using AdeelBrotherCement.Application.Interfaces;
using AdeelBrotherCement.Domain.Entities;

namespace AdeelBrotherCement.Application.Services;

public class CustomerService(
    ICustomerRepository customerRepository,
    ICustomerPaymentRepository paymentRepository,
    ITransactionRepository transactionRepository)
{
    public async Task<IReadOnlyList<CustomerDto>> GetAllAsync(CancellationToken ct = default)
    {
        var customers = await customerRepository.GetAllAsync(ct);
        return customers.Select(Map).OrderByDescending(c => c.Balance).ThenBy(c => c.Name).ToList();
    }

    public async Task<IReadOnlyList<CustomerDto>> GetWithBalanceAsync(CancellationToken ct = default)
    {
        var customers = await customerRepository.GetAllAsync(ct);
        return customers
            .Where(c => c.Balance > 0)
            .Select(Map)
            .OrderByDescending(c => c.Balance)
            .ToList();
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await customerRepository.GetByIdAsync(id, ct);
        return customer is null ? null : Map(customer);
    }

    public async Task<IReadOnlyList<CustomerDto>> SearchAsync(string query, CancellationToken ct = default)
    {
        var customers = await customerRepository.SearchAsync(query, ct);
        return customers.Select(Map).ToList();
    }

    public async Task<CustomerLookupDto> LookupAsync(string name, string mobile, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(mobile))
            return new CustomerLookupDto(false, null, "Name and mobile are required.");

        var existing = await customerRepository.GetByNameAndPhoneAsync(name, mobile, ct);
        if (existing is null)
            return new CustomerLookupDto(false, null, "New customer — Khata Book will be created on first sale.");

        return new CustomerLookupDto(true, Map(existing),
            $"Existing customer found. Current balance: {existing.Balance:N0} PKR.");
    }

    public async Task<KhataBookDto?> GetKhataBookAsync(Guid customerId, CancellationToken ct = default)
    {
        var customer = await customerRepository.GetByIdAsync(customerId, ct);
        if (customer is null) return null;

        var entries = await BuildKhataEntriesAsync(customer, ct);
        return new KhataBookDto(Map(customer), entries, customer.Balance);
    }

    public async Task<CustomerHistoryDto?> GetHistoryAsync(Guid customerId, CancellationToken ct = default)
    {
        var customer = await customerRepository.GetByIdAsync(customerId, ct);
        if (customer is null) return null;

        var sales = await GetSalesForCustomerAsync(customer, ct);
        var payments = await GetPaymentsAsync(customerId, ct);

        return new CustomerHistoryDto(Map(customer), sales, payments);
    }

    public async Task<IReadOnlyList<CustomerPaymentDto>> GetPaymentsAsync(Guid customerId, CancellationToken ct = default)
    {
        var payments = await paymentRepository.GetByCustomerIdAsync(customerId, ct);
        return payments.Select(MapPayment).OrderByDescending(p => p.PaymentDate).ToList();
    }

    public async Task<CustomerPaymentDto> RecordPaymentAsync(Guid customerId, RecordPaymentRequest request, CancellationToken ct = default)
    {
        if (request.Amount <= 0)
            throw new InvalidOperationException("Payment amount must be greater than zero.");

        var customer = await customerRepository.GetByIdAsync(customerId, ct)
            ?? throw new InvalidOperationException("Customer not found.");

        if (request.Amount > customer.Balance)
            throw new InvalidOperationException(
                $"Payment exceeds balance. Customer owes {customer.Balance:N0} PKR.");

        await AllocatePaymentToSalesAsync(customer, request.Amount, ct);

        customer.Balance -= request.Amount;
        await customerRepository.UpdateAsync(customer, ct);

        var payment = new CustomerPayment
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            Amount = request.Amount,
            PaymentDate = request.PaymentDate ?? DateTime.Now,
            Notes = request.Notes?.Trim()
        };

        var created = await paymentRepository.CreateAsync(payment, ct);
        return MapPayment(created);
    }

    private async Task AllocatePaymentToSalesAsync(Customer customer, decimal amount, CancellationToken ct)
    {
        var normalizedPhone = NormalizePhone(customer.Phone ?? "");
        var transactions = await transactionRepository.GetAllAsync(ct);
        var unpaidSales = transactions
            .Where(t => t.BalanceDue > 0 &&
                (t.CustomerId == customer.Id ||
                 (!string.IsNullOrEmpty(normalizedPhone) &&
                  NormalizePhone(t.CustomerMobile) == normalizedPhone)))
            .OrderBy(t => t.TransactionDate)
            .ToList();

        var remaining = amount;
        foreach (var sale in unpaidSales)
        {
            if (remaining <= 0) break;

            var applied = Math.Min(remaining, sale.BalanceDue);
            sale.AmountPaid += applied;
            sale.BalanceDue -= applied;
            await transactionRepository.UpdateAsync(sale, ct);
            remaining -= applied;
        }
    }

    internal async Task<Customer> FindOrCreateAsync(string name, string mobile, CancellationToken ct = default)
    {
        var normalizedPhone = NormalizePhone(mobile);
        var existing = await customerRepository.GetByNameAndPhoneAsync(name, normalizedPhone, ct);

        if (existing is not null)
        {
            existing.Name = name.Trim();
            existing.Phone = normalizedPhone;
            return await customerRepository.UpdateAsync(existing, ct);
        }

        var byPhone = await customerRepository.GetByPhoneAsync(normalizedPhone, ct);
        if (byPhone is not null &&
            NormalizeName(byPhone.Name) == NormalizeName(name))
        {
            byPhone.Phone = normalizedPhone;
            return await customerRepository.UpdateAsync(byPhone, ct);
        }

        var customer = new Customer
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Phone = normalizedPhone,
            Balance = 0
        };

        return await customerRepository.CreateAsync(customer, ct);
    }

    internal static string NormalizePhone(string phone) =>
        new string(phone.Where(c => char.IsDigit(c) || c == '+').ToArray());

    internal static string NormalizeName(string name) =>
        name.Trim().ToLowerInvariant();

    private async Task<IReadOnlyList<KhataEntryDto>> BuildKhataEntriesAsync(Customer customer, CancellationToken ct)
    {
        var sales = await GetSalesForCustomerAsync(customer, ct);
        var payments = await GetPaymentsAsync(customer.Id, ct);

        var events = new List<(DateTime Date, string Type, string Description, string? Ref, decimal Purchase, decimal Payment)>();

        foreach (var sale in sales.OrderBy(s => s.TransactionDate))
        {
            events.Add((
                sale.TransactionDate,
                "Purchase",
                $"Invoice {sale.SlipNumber}",
                sale.SlipNumber,
                sale.TotalAmount,
                sale.AmountPaid));
        }

        foreach (var payment in payments.OrderBy(p => p.PaymentDate))
        {
            events.Add((
                payment.PaymentDate,
                "Payment",
                payment.Notes ?? "Payment received",
                payment.Id.ToString(),
                0,
                payment.Amount));
        }

        var sorted = events.OrderBy(e => e.Date).ThenBy(e => e.Type).ToList();
        var running = 0m;
        var entries = new List<KhataEntryDto>();

        foreach (var e in sorted)
        {
            var previous = running;
            if (e.Type == "Purchase")
            {
                running += e.Purchase - e.Payment;
                entries.Add(new KhataEntryDto(
                    e.Date, e.Type, e.Description, e.Ref,
                    previous, e.Purchase, e.Payment, running));
            }
            else
            {
                running -= e.Payment;
                entries.Add(new KhataEntryDto(
                    e.Date, e.Type, e.Description, e.Ref,
                    previous, 0, e.Payment, running));
            }
        }

        return entries;
    }

    private async Task<IReadOnlyList<SaleDto>> GetSalesForCustomerAsync(Customer customer, CancellationToken ct)
    {
        var normalizedPhone = NormalizePhone(customer.Phone ?? "");
        var transactions = await transactionRepository.GetAllAsync(ct);

        return transactions
            .Where(t =>
                t.CustomerId == customer.Id ||
                (!string.IsNullOrEmpty(normalizedPhone) &&
                 NormalizePhone(t.CustomerMobile) == normalizedPhone))
            .OrderByDescending(t => t.TransactionDate)
            .Select(MapSale)
            .ToList();
    }

    private static SaleDto MapSale(SaleTransaction t) => new(
        t.Id,
        t.SlipNumber,
        t.CustomerId,
        t.CustomerName,
        t.CustomerMobile,
        t.TransactionDate,
        t.TotalAmount,
        t.AmountPaid,
        t.BalanceDue,
        t.PreviousBalance,
        t.TotalCost,
        t.TotalAmount - t.TotalCost,
        t.Notes,
        t.Items.Select(i => new SaleItemDto(
            i.ProductId, i.ProductName, i.Quantity,
            i.UnitPrice, i.UnitCost, i.LineTotal, i.LineProfit)).ToList());

    private static CustomerDto Map(Customer c) => new(c.Id, c.Name, c.Phone, c.Address, c.Balance);

    private static CustomerPaymentDto MapPayment(CustomerPayment p) => new(
        p.Id, p.CustomerId, p.CustomerName, p.Amount, p.PaymentDate, p.Notes);
}
