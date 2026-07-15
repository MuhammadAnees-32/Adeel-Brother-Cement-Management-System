using AdeelBrotherCement.Application.DTOs;
using AdeelBrotherCement.Application.Interfaces;

namespace AdeelBrotherCement.Application.Services;

public class ReportService(
    ITransactionRepository transactionRepository,
    ICustomerRepository customerRepository,
    IDealerRepository dealerRepository,
    IDealerPurchaseRepository purchaseRepository,
    IExpenseRepository expenseRepository,
    IAdvanceBookingRepository bookingRepository,
    TransactionService transactionService,
    InventoryService inventoryService)
{
    public async Task<SalesReportDto> GetDailySalesAsync(DateTime? date, CancellationToken ct = default)
    {
        var target = (date ?? DateTime.Today).Date;
        var from = target;
        var to = target.AddDays(1).AddTicks(-1);
        return await BuildSalesReport("Daily Sales Report", from, to, ct);
    }

    public async Task<SalesReportDto> GetMonthlySalesAsync(int? year, int? month, CancellationToken ct = default)
    {
        var y = year ?? DateTime.Today.Year;
        var m = month ?? DateTime.Today.Month;
        var from = new DateTime(y, m, 1);
        var to = from.AddMonths(1).AddTicks(-1);
        return await BuildSalesReport($"Monthly Sales Report — {from:MMMM yyyy}", from, to, ct);
    }

    public async Task<SalesReportDto> GetSalesReportAsync(DateTime from, DateTime to, CancellationToken ct = default)
        => await BuildSalesReport($"Sales Report — {from:dd MMM yyyy} to {to:dd MMM yyyy}", from, to, ct);

    public async Task<CustomerBalanceReportDto> GetCustomerBalanceReportAsync(CancellationToken ct = default)
    {
        var customers = await customerRepository.GetAllAsync(ct);
        var withBalance = customers
            .Where(c => c.Balance > 0)
            .Select(c => new CustomerDto(c.Id, c.Name, c.Phone, c.Address, c.Balance))
            .OrderByDescending(c => c.Balance)
            .ToList();

        return new CustomerBalanceReportDto(withBalance, withBalance.Sum(c => c.Balance));
    }

    public async Task<DealerOutstandingReportDto> GetDealerOutstandingReportAsync(CancellationToken ct = default)
    {
        var dealers = await dealerRepository.GetAllAsync(ct);
        var dtos = dealers
            .Select(d => new DealerDto(d.Id, d.Name, d.Phone, d.Address, d.OutstandingBalance))
            .OrderByDescending(d => d.OutstandingBalance)
            .ToList();

        return new DealerOutstandingReportDto(dtos, dtos.Sum(d => d.OutstandingBalance));
    }

    public async Task<InventoryReportDto> GetInventoryReportAsync(CancellationToken ct = default)
    {
        var items = await inventoryService.GetInventoryAsync(ct);
        return new InventoryReportDto(items, items.Sum(i => i.StockValue));
    }

    public async Task<LowStockReportDto> GetLowStockReportAsync(decimal threshold = 10, CancellationToken ct = default)
    {
        var items = await inventoryService.GetInventoryAsync(ct);
        var low = items.Where(i => i.StockQuantity <= threshold).OrderBy(i => i.StockQuantity).ToList();
        return new LowStockReportDto(low);
    }

    public async Task<PurchaseReportDto> GetPurchaseReportAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var purchases = (await purchaseRepository.GetAllAsync(ct))
            .Where(p => p.PurchaseDate >= from && p.PurchaseDate <= to)
            .OrderByDescending(p => p.PurchaseDate)
            .Select(p => new DealerPurchaseDto(
                p.Id, p.DealerId, p.DealerName, p.ProductId, p.ProductName,
                p.Quantity, p.UnitPrice, p.TotalAmount, p.AmountPaid, p.BalanceDue,
                p.PurchaseDate, p.Notes))
            .ToList();

        return new PurchaseReportDto(
            from, to, purchases,
            purchases.Sum(p => p.TotalAmount),
            purchases.Sum(p => p.AmountPaid),
            purchases.Sum(p => p.BalanceDue));
    }

    public async Task<ProfitReportDto> GetProfitReportAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        var sales = await transactionRepository.GetByDateRangeAsync(from, to, ct);
        var totalSales = sales.Sum(s => s.TotalAmount);
        var totalCost = sales.Sum(s => s.TotalCost);
        var grossProfit = totalSales - totalCost;

        var expenses = await expenseRepository.GetByDateRangeAsync(from, to, ct);
        var totalExpenses = expenses.Sum(e => e.Amount);

        return new ProfitReportDto(from, to, totalSales, totalCost, grossProfit, totalExpenses, grossProfit - totalExpenses);
    }

    public async Task<AdvanceBookingReportDto> GetAdvanceBookingReportAsync(CancellationToken ct = default)
    {
        var bookings = await bookingRepository.GetAllAsync(ct);
        var dtos = bookings.Select(b => new AdvanceBookingDto(
            b.Id, b.CustomerId, b.CustomerName, b.CustomerMobile,
            b.ProductId, b.ProductName, b.Quantity, b.UnitPrice,
            b.TotalAmount, b.AdvancePaid, b.RemainingAmount,
            b.DeliveryDate, b.BookedDate, b.Status.ToString(),
            b.InvoiceId, b.Notes)).ToList();

        return new AdvanceBookingReportDto(
            dtos.Where(b => b.Status == "Pending").ToList(),
            dtos.Where(b => b.Status == "Delivered").ToList(),
            dtos);
    }

    private async Task<SalesReportDto> BuildSalesReport(string title, DateTime from, DateTime to, CancellationToken ct)
    {
        var transactions = await transactionRepository.GetByDateRangeAsync(from, to, ct);
        var sales = transactions
            .Select(t => transactionService.MapSale(t))
            .OrderByDescending(s => s.TransactionDate)
            .ToList();

        return new SalesReportDto(
            title, from, to,
            sales.Sum(s => s.TotalAmount),
            sales.Sum(s => s.TotalCost),
            sales.Sum(s => s.TotalProfit),
            sales.Count,
            sales);
    }
}
