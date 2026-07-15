namespace AdeelBrotherCement.Application.DTOs;

public record ProductDto(
    Guid Id,
    string Category,
    string Name,
    string Unit,
    decimal PurchasePrice,
    decimal SalePrice,
    decimal StockQuantity,
    bool IsActive,
    Guid? DealerId = null,
    string? DealerName = null,
    decimal TotalPurchased = 0,
    decimal TotalSold = 0);

public record UpdateProductRequest(
    string Name,
    string Unit,
    Guid? DealerId,
    decimal PurchasePrice,
    decimal SalePrice,
    decimal StockQuantity);

public record CreateProductRequest(
    string Category,
    string Name,
    string Unit,
    decimal PurchasePrice,
    decimal SalePrice,
    decimal StockQuantity,
    Guid? DealerId = null);

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
    List<SaleItemRequest> Items,
    decimal LoadingCharge = 0,
    decimal TransportCharge = 0,
    string? DriverName = null,
    string? VehicleNumber = null,
    decimal? TotalWeight = null);

public record SaleItemDto(
    Guid ProductId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal UnitCost,
    decimal LineTotal,
    decimal LineProfit,
    string Unit = "");

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
    decimal PreviousBalance,
    decimal TotalCost,
    decimal TotalProfit,
    string? Notes,
    List<SaleItemDto> Items,
    decimal LoadingCharge = 0,
    decimal TransportCharge = 0,
    decimal TotalWeight = 0,
    string? DriverName = null,
    string? VehicleNumber = null)
{
    public decimal BillTotal => TotalAmount + LoadingCharge + TransportCharge;
}

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
    decimal StockValue,
    Guid? DealerId = null,
    string? DealerName = null,
    decimal TotalPurchased = 0,
    decimal TotalSold = 0,
    decimal RemainingStock = 0);

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

public record CustomerLookupDto(
    bool Exists,
    CustomerDto? Customer,
    string Message);

public record KhataEntryDto(
    DateTime Date,
    string Type,
    string Description,
    string? Reference,
    decimal PreviousBalance,
    decimal PurchaseAmount,
    decimal PaymentReceived,
    decimal RemainingBalance);

public record KhataBookDto(
    CustomerDto Customer,
    IReadOnlyList<KhataEntryDto> Entries,
    decimal CurrentBalance);

public record DealerDto(
    Guid Id,
    string Name,
    string? Phone,
    string? Address,
    decimal OutstandingBalance);

public record CreateDealerRequest(string Name, string? Phone, string? Address);

public record DealerPurchaseDto(
    Guid Id,
    Guid DealerId,
    string DealerName,
    Guid ProductId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal TotalAmount,
    decimal AmountPaid,
    decimal BalanceDue,
    DateTime PurchaseDate,
    string? Notes);

public record CreateDealerPurchaseRequest(
    Guid DealerId,
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice,
    decimal? AmountPaid,
    DateTime? PurchaseDate,
    string? Notes);

public record DealerPaymentDto(
    Guid Id,
    Guid DealerId,
    string DealerName,
    decimal Amount,
    DateTime PaymentDate,
    string? Notes);

public record RecordDealerPaymentRequest(decimal Amount, DateTime? PaymentDate, string? Notes);

public record DealerHistoryDto(
    DealerDto Dealer,
    IReadOnlyList<DealerPurchaseDto> Purchases,
    IReadOnlyList<DealerPaymentDto> Payments);

public record AdvanceBookingDto(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    string CustomerMobile,
    Guid ProductId,
    string ProductName,
    decimal Quantity,
    decimal UnitPrice,
    decimal TotalAmount,
    decimal AdvancePaid,
    decimal RemainingAmount,
    DateTime DeliveryDate,
    DateTime BookedDate,
    string Status,
    Guid? InvoiceId,
    string? Notes);

public record CreateAdvanceBookingRequest(
    string CustomerName,
    string CustomerMobile,
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice,
    decimal AdvancePaid,
    DateTime DeliveryDate,
    string? Notes);

public record ReportRequest(DateTime? From, DateTime? To);

public record SalesReportDto(
    string Title,
    DateTime From,
    DateTime To,
    decimal TotalSales,
    decimal TotalCost,
    decimal TotalProfit,
    int TransactionCount,
    IReadOnlyList<SaleDto> Sales);

public record CustomerBalanceReportDto(
    IReadOnlyList<CustomerDto> Customers,
    decimal TotalOutstanding);

public record DealerOutstandingReportDto(
    IReadOnlyList<DealerDto> Dealers,
    decimal TotalOutstanding);

public record InventoryReportDto(
    IReadOnlyList<InventoryItemDto> Items,
    decimal TotalStockValue);

public record LowStockReportDto(
    IReadOnlyList<InventoryItemDto> Items);

public record PurchaseReportDto(
    DateTime From,
    DateTime To,
    IReadOnlyList<DealerPurchaseDto> Purchases,
    decimal TotalPurchases,
    decimal TotalPaid,
    decimal TotalOutstanding);

public record ProfitReportDto(
    DateTime From,
    DateTime To,
    decimal TotalSales,
    decimal TotalCost,
    decimal GrossProfit,
    decimal TotalExpenses,
    decimal NetProfit);

public record AdvanceBookingReportDto(
    IReadOnlyList<AdvanceBookingDto> Pending,
    IReadOnlyList<AdvanceBookingDto> Delivered,
    IReadOnlyList<AdvanceBookingDto> All);

public record ShopPurchaseDto(
    Guid Id,
    string ShopName,
    string ItemName,
    decimal Quantity,
    string Unit,
    decimal UnitPrice,
    decimal TotalAmount,
    decimal AmountPaid,
    decimal BalanceDue,
    DateTime PurchaseDate,
    string? Notes,
    string PaymentStatus);

public record CreateShopPurchaseRequest(
    string ShopName,
    string ItemName,
    decimal Quantity,
    string? Unit,
    decimal UnitPrice,
    decimal? AmountPaid,
    DateTime? PurchaseDate,
    string? Notes);
