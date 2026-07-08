namespace AdeelBrotherCement.Application.DTOs;

public record ProductDto(
    Guid Id,
    string Category,
    string Name,
    string Unit,
    decimal PurchasePrice,
    decimal SalePrice,
    decimal StockQuantity,
    bool IsActive);

public record UpdateProductRequest(
    decimal PurchasePrice,
    decimal SalePrice,
    decimal StockQuantity);

public record CreateProductRequest(
    string Category,
    string Name,
    string Unit,
    decimal PurchasePrice,
    decimal SalePrice,
    decimal StockQuantity);

public record CustomerDto(Guid Id, string Name, string? Phone, string? Address, decimal Balance);

public record CreateCustomerRequest(string Name, string? Phone, string? Address);

public record RecordPaymentRequest(decimal Amount, DateTime? PaymentDate, string? Notes);

public record CustomerPaymentDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    decimal Amount,
    DateTime PaymentDate,
    string? Notes);

public record CustomerHistoryDto(
    CustomerDto Customer,
    IReadOnlyList<SaleDto> Sales,
    IReadOnlyList<CustomerPaymentDto> Payments);

public record SaleItemRequest(Guid ProductId, decimal Quantity, decimal? UnitPrice);

public record CreateSaleRequest(
    string CustomerName,
    string CustomerMobile,
    Guid? CustomerId,
    DateTime? TransactionDate,
    decimal? AmountPaid,
    string? Notes,
    List<SaleItemRequest> Items);

public record SaleItemDto(
    Guid ProductId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal UnitCost,
    decimal LineTotal,
    decimal LineProfit);

public record SaleDto(
    Guid Id,
    string SlipNumber,
    Guid? CustomerId,
    string CustomerName,
    string CustomerMobile,
    DateTime TransactionDate,
    decimal TotalAmount,
    decimal AmountPaid,
    decimal BalanceDue,
    decimal TotalCost,
    decimal TotalProfit,
    string? Notes,
    List<SaleItemDto> Items);

public record ExpenseDto(Guid Id, DateTime ExpenseDate, string Category, string Description, decimal Amount);

public record CreateExpenseRequest(DateTime ExpenseDate, string Category, string Description, decimal Amount);

public record StockAdjustmentRequest(decimal Quantity, string Reason);

public record InventoryItemDto(
    Guid Id,
    string Category,
    string Name,
    string Unit,
    decimal StockQuantity,
    decimal PurchasePrice,
    decimal SalePrice,
    decimal StockValue);

public record SalesSummaryDto(string Period, decimal TotalSales, decimal TotalCost, decimal TotalProfit, int TransactionCount);

public record ProductSalesDto(string ProductName, string Category, decimal QuantitySold, decimal TotalSales, decimal TotalProfit);

public record DashboardDto(
    decimal TodaySales,
    decimal TodayProfit,
    decimal TodayExpenses,
    decimal NetProfitToday,
    decimal WeekSales,
    decimal MonthSales,
    decimal YearSales,
    decimal TotalExpensesThisMonth,
    decimal NetProfitThisMonth,
    decimal TotalOutstanding,
    List<InventoryItemDto> Inventory,
    List<SalesSummaryDto> SalesByPeriod,
    List<ProductSalesDto> TopProducts,
    List<CustomerDto> CustomersWithBalance);
