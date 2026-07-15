using AdeelBrotherCement.Api.Authorization;
using AdeelBrotherCement.Application.DTOs;
using AdeelBrotherCement.Application.Interfaces;
using AdeelBrotherCement.Application.Services;
using AdeelBrotherCement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdeelBrotherCement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
        => Ok(new { status = "ok", timestamp = DateTime.Now });
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
[RequireScreen(AppScreen.Dashboard)]
public class DashboardController(DashboardService dashboardService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get(CancellationToken ct)
        => Ok(await dashboardService.GetDashboardAsync(ct));
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController(ProductService productService) : ControllerBase
{
    [HttpGet]
    [RequireScreen(AppScreen.Inventory, AppScreen.NewSale)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll([FromQuery] string? category, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(category))
            return Ok(await productService.GetByCategoryAsync(category, ct));

        return Ok(await productService.GetAllAsync(ct));
    }

    [HttpPut("{id:guid}")]
    [RequireScreen(AppScreen.Inventory)]
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        try
        {
            var product = await productService.UpdateAsync(id, request, ct);
            return product is null ? NotFound() : Ok(product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost]
    [RequireScreen(AppScreen.Inventory)]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        try
        {
            var product = await productService.CreateAsync(request, ct);
            return Created(string.Empty, product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    [RequireScreen(AppScreen.Inventory)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            return await productService.DeleteAsync(id, ct) ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesController(TransactionService transactionService) : ControllerBase
{
    [HttpGet]
    [RequireScreen(AppScreen.SalesHistory)]
    public async Task<ActionResult<IReadOnlyList<SaleDto>>> GetAll(CancellationToken ct)
        => Ok(await transactionService.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    [RequireScreen(AppScreen.SalesHistory, AppScreen.NewSale)]
    public async Task<ActionResult<SaleDto>> GetById(Guid id, CancellationToken ct)
    {
        var sale = await transactionService.GetByIdAsync(id, ct);
        return sale is null ? NotFound() : Ok(sale);
    }

    [HttpPost]
    [RequireScreen(AppScreen.NewSale)]
    public async Task<ActionResult<SaleDto>> Create([FromBody] CreateSaleRequest request, CancellationToken ct)
    {
        try
        {
            var sale = await transactionService.CreateAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id = sale.Id }, sale);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
[RequireScreen(AppScreen.CustomerBalance)]
public class CustomersController(CustomerService customerService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CustomerDto>>> GetAll(
        [FromQuery] bool balanceOnly, CancellationToken ct)
    {
        var customers = balanceOnly
            ? await customerService.GetWithBalanceAsync(ct)
            : await customerService.GetAllAsync(ct);
        return Ok(customers);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CustomerDto>> GetById(Guid id, CancellationToken ct)
    {
        var customer = await customerService.GetByIdAsync(id, ct);
        return customer is null ? NotFound() : Ok(customer);
    }

    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<CustomerHistoryDto>> GetHistory(Guid id, CancellationToken ct)
    {
        var history = await customerService.GetHistoryAsync(id, ct);
        return history is null ? NotFound() : Ok(history);
    }

    [HttpGet("{id:guid}/payments")]
    public async Task<ActionResult<IReadOnlyList<CustomerPaymentDto>>> GetPayments(Guid id, CancellationToken ct)
        => Ok(await customerService.GetPaymentsAsync(id, ct));

    [HttpGet("search")]
    public async Task<ActionResult<IReadOnlyList<CustomerDto>>> Search([FromQuery] string q, CancellationToken ct)
        => Ok(await customerService.SearchAsync(q, ct));

    [HttpGet("lookup")]
    public async Task<ActionResult<CustomerLookupDto>> Lookup(
        [FromQuery] string name, [FromQuery] string mobile, CancellationToken ct)
        => Ok(await customerService.LookupAsync(name, mobile, ct));

    [HttpGet("{id:guid}/khata")]
    public async Task<ActionResult<KhataBookDto>> GetKhata(Guid id, CancellationToken ct)
    {
        var khata = await customerService.GetKhataBookAsync(id, ct);
        return khata is null ? NotFound() : Ok(khata);
    }

    [HttpPost("{id:guid}/payments")]
    public async Task<ActionResult<CustomerPaymentDto>> RecordPayment(
        Guid id, [FromBody] RecordPaymentRequest request, CancellationToken ct)
    {
        try
        {
            var payment = await customerService.RecordPaymentAsync(id, request, ct);
            return Created(string.Empty, payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
[RequireScreen(AppScreen.Inventory)]
public class InventoryController(InventoryService inventoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InventoryItemDto>>> GetAll(CancellationToken ct)
        => Ok(await inventoryService.GetInventoryAsync(ct));

    [HttpPost("{productId:guid}/adjust")]
    public async Task<ActionResult<InventoryItemDto>> Adjust(Guid productId, [FromBody] StockAdjustmentRequest request, CancellationToken ct)
    {
        var item = await inventoryService.AdjustStockAsync(productId, request, ct);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpPut("{productId:guid}/set")]
    public async Task<ActionResult<InventoryItemDto>> Set(Guid productId, [FromBody] StockAdjustmentRequest request, CancellationToken ct)
    {
        var item = await inventoryService.SetStockAsync(productId, request, ct);
        return item is null ? NotFound() : Ok(item);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
[RequireScreen(AppScreen.Expenses)]
public class ExpensesController(ExpenseService expenseService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExpenseDto>>> GetAll(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken ct)
    {
        if (from.HasValue && to.HasValue)
            return Ok(await expenseService.GetByDateRangeAsync(from.Value, to.Value, ct));

        return Ok(await expenseService.GetAllAsync(ct));
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create([FromBody] CreateExpenseRequest request, CancellationToken ct)
    {
        var expense = await expenseService.CreateAsync(request, ct);
        return Created(string.Empty, expense);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        => await expenseService.DeleteAsync(id, ct) ? NoContent() : NotFound();
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
[RequireScreen(AppScreen.Dashboard)]
public class BackupController(IBackupService backupService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<BackupResult>> Create(CancellationToken ct)
    {
        try
        {
            var result = await backupService.CreateDailyBackupAsync(ct);
            return Ok(result);
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public ActionResult<IReadOnlyList<BackupInfo>> List([FromQuery] int limit = 10)
        => Ok(backupService.GetRecentBackups(limit));
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
[RequireScreen(AppScreen.Dashboard)]
public class SyncController(ISyncService syncService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<SyncResult>> Sync(CancellationToken ct)
    {
        try
        {
            var result = await syncService.SyncAsync(ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("status")]
    public ActionResult<SyncStatus> Status() => Ok(syncService.GetStatus());
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DealersController(DealerService dealerService) : ControllerBase
{
    [HttpGet]
    [RequireScreen(AppScreen.Dealers, AppScreen.Inventory)]
    public async Task<ActionResult<IReadOnlyList<DealerDto>>> GetAll(CancellationToken ct)
        => Ok(await dealerService.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    [RequireScreen(AppScreen.Dealers)]
    public async Task<ActionResult<DealerDto>> GetById(Guid id, CancellationToken ct)
    {
        var dealer = await dealerService.GetByIdAsync(id, ct);
        return dealer is null ? NotFound() : Ok(dealer);
    }

    [HttpGet("{id:guid}/history")]
    [RequireScreen(AppScreen.Dealers)]
    public async Task<ActionResult<DealerHistoryDto>> GetHistory(Guid id, CancellationToken ct)
    {
        var history = await dealerService.GetHistoryAsync(id, ct);
        return history is null ? NotFound() : Ok(history);
    }

    [HttpPost]
    [RequireScreen(AppScreen.Dealers)]
    public async Task<ActionResult<DealerDto>> Create([FromBody] CreateDealerRequest request, CancellationToken ct)
    {
        try
        {
            var dealer = await dealerService.CreateAsync(request, ct);
            return Created(string.Empty, dealer);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("purchases")]
    [RequireScreen(AppScreen.Dealers)]
    public async Task<ActionResult<DealerPurchaseDto>> RecordPurchase(
        [FromBody] CreateDealerPurchaseRequest request, CancellationToken ct)
    {
        try
        {
            var purchase = await dealerService.RecordPurchaseAsync(request, ct);
            return Created(string.Empty, purchase);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/payments")]
    [RequireScreen(AppScreen.Dealers)]
    public async Task<ActionResult<DealerPaymentDto>> RecordPayment(
        Guid id, [FromBody] RecordDealerPaymentRequest request, CancellationToken ct)
    {
        try
        {
            var payment = await dealerService.RecordPaymentAsync(id, request, ct);
            return Created(string.Empty, payment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
[RequireScreen(AppScreen.AdvanceBookings)]
public class BookingsController(BookingService bookingService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AdvanceBookingDto>>> GetAll(CancellationToken ct)
        => Ok(await bookingService.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdvanceBookingDto>> GetById(Guid id, CancellationToken ct)
    {
        var booking = await bookingService.GetByIdAsync(id, ct);
        return booking is null ? NotFound() : Ok(booking);
    }

    [HttpPost]
    public async Task<ActionResult<AdvanceBookingDto>> Create(
        [FromBody] CreateAdvanceBookingRequest request, CancellationToken ct)
    {
        try
        {
            var booking = await bookingService.CreateAsync(request, ct);
            return Created(string.Empty, booking);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/deliver")]
    public async Task<ActionResult<SaleDto>> Deliver(
        Guid id, [FromBody] RecordPaymentRequest? request, CancellationToken ct)
    {
        try
        {
            var sale = await bookingService.DeliverAsync(id, request?.Amount, ct);
            return Ok(sale);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
[RequireScreen(AppScreen.Reports)]
public class ReportsController(ReportService reportService) : ControllerBase
{
    [HttpGet("daily-sales")]
    public async Task<ActionResult<SalesReportDto>> DailySales([FromQuery] DateTime? date, CancellationToken ct)
        => Ok(await reportService.GetDailySalesAsync(date, ct));

    [HttpGet("monthly-sales")]
    public async Task<ActionResult<SalesReportDto>> MonthlySales(
        [FromQuery] int? year, [FromQuery] int? month, CancellationToken ct)
        => Ok(await reportService.GetMonthlySalesAsync(year, month, ct));

    [HttpGet("sales")]
    public async Task<ActionResult<SalesReportDto>> Sales(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct)
        => Ok(await reportService.GetSalesReportAsync(from, to, ct));

    [HttpGet("customer-balances")]
    public async Task<ActionResult<CustomerBalanceReportDto>> CustomerBalances(CancellationToken ct)
        => Ok(await reportService.GetCustomerBalanceReportAsync(ct));

    [HttpGet("dealer-outstanding")]
    public async Task<ActionResult<DealerOutstandingReportDto>> DealerOutstanding(CancellationToken ct)
        => Ok(await reportService.GetDealerOutstandingReportAsync(ct));

    [HttpGet("inventory")]
    public async Task<ActionResult<InventoryReportDto>> Inventory(CancellationToken ct)
        => Ok(await reportService.GetInventoryReportAsync(ct));

    [HttpGet("low-stock")]
    public async Task<ActionResult<LowStockReportDto>> LowStock(CancellationToken ct)
        => Ok(await reportService.GetLowStockReportAsync(ct: ct));

    [HttpGet("purchases")]
    public async Task<ActionResult<PurchaseReportDto>> Purchases(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct)
        => Ok(await reportService.GetPurchaseReportAsync(from, to, ct));

    [HttpGet("profit")]
    public async Task<ActionResult<ProfitReportDto>> Profit(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct)
        => Ok(await reportService.GetProfitReportAsync(from, to, ct));

    [HttpGet("advance-bookings")]
    public async Task<ActionResult<AdvanceBookingReportDto>> AdvanceBookings(CancellationToken ct)
        => Ok(await reportService.GetAdvanceBookingReportAsync(ct));
}
