using AdeelBrotherCement.Application.DTOs;
using AdeelBrotherCement.Application.Interfaces;

namespace AdeelBrotherCement.Application.Services;

public class DashboardService(
    ITransactionRepository transactionRepository,
    IExpenseRepository expenseRepository,
    IProductRepository productRepository,
    ICustomerRepository customerRepository)
{
    public async Task<DashboardDto> GetDashboardAsync(CancellationToken ct = default)
    {
        var now = DateTime.Now;
        var today = now.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var yearStart = new DateTime(today.Year, 1, 1);

        var allTransactions = await transactionRepository.GetAllAsync(ct);
        var allExpenses = await expenseRepository.GetAllAsync(ct);
        var products = await productRepository.GetAllAsync(ct);

        var todayTx = allTransactions.Where(t => t.TransactionDate.Date == today).ToList();
        var weekTx = allTransactions.Where(t => t.TransactionDate >= weekStart).ToList();
        var monthTx = allTransactions.Where(t => t.TransactionDate >= monthStart).ToList();
        var yearTx = allTransactions.Where(t => t.TransactionDate >= yearStart).ToList();

        var todayExpenses = allExpenses.Where(e => e.ExpenseDate.Date == today).Sum(e => e.Amount);
        var monthExpenses = allExpenses.Where(e => e.ExpenseDate >= monthStart).Sum(e => e.Amount);

        var todaySales = todayTx.Sum(t => t.TotalAmount);
        var todayProfit = todayTx.Sum(t => t.TotalAmount - t.TotalCost);

        var inventory = products
            .Where(p => p.IsActive)
            .Select(p => new InventoryItemDto(
                p.Id, p.Category.ToString(), p.Name, p.Unit,
                p.StockQuantity, p.PurchasePrice, p.SalePrice,
                p.StockQuantity * p.PurchasePrice))
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .ToList();

        var salesByPeriod = new List<SalesSummaryDto>
        {
            BuildSummary("Today", todayTx),
            BuildSummary("This Week", weekTx),
            BuildSummary("This Month", monthTx),
            BuildSummary("This Year", yearTx)
        };

        var topProducts = monthTx
            .SelectMany(t => t.Items)
            .GroupBy(i => new { i.ProductName, ProductId = i.ProductId })
            .Select(g =>
            {
                var product = products.FirstOrDefault(p => p.Id == g.Key.ProductId);
                return new ProductSalesDto(
                    g.Key.ProductName,
                    product?.Category.ToString() ?? "Unknown",
                    g.Sum(i => i.Quantity),
                    g.Sum(i => i.LineTotal),
                    g.Sum(i => i.LineProfit));
            })
            .OrderByDescending(p => p.TotalSales)
            .Take(10)
            .ToList();

        var monthProfit = monthTx.Sum(t => t.TotalAmount - t.TotalCost);

        var customersWithBalance = (await customerRepository.GetAllAsync(ct))
            .Where(c => c.Balance > 0)
            .Select(c => new CustomerDto(c.Id, c.Name, c.Phone, c.Address, c.Balance))
            .OrderByDescending(c => c.Balance)
            .ToList();

        var totalOutstanding = customersWithBalance.Sum(c => c.Balance);

        return new DashboardDto(
            todaySales,
            todayProfit,
            todayExpenses,
            todayProfit - todayExpenses,
            weekTx.Sum(t => t.TotalAmount),
            monthTx.Sum(t => t.TotalAmount),
            yearTx.Sum(t => t.TotalAmount),
            monthExpenses,
            monthProfit - monthExpenses,
            totalOutstanding,
            inventory,
            salesByPeriod,
            topProducts,
            customersWithBalance);
    }

    private static SalesSummaryDto BuildSummary(string period, List<Domain.Entities.SaleTransaction> transactions) =>
        new(
            period,
            transactions.Sum(t => t.TotalAmount),
            transactions.Sum(t => t.TotalCost),
            transactions.Sum(t => t.TotalAmount - t.TotalCost),
            transactions.Count);
}
