using AdeelBrotherCement.Application.DTOs;
using AdeelBrotherCement.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdeelBrotherCement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController(DashboardService dashboardService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardDto>> Get(CancellationToken ct)
        => Ok(await dashboardService.GetDashboardAsync(ct));
}

[ApiController]
[Route("api/[controller]")]
public class ProductsController(ProductService productService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll([FromQuery] string? category, CancellationToken ct)
    {
        if (!string.IsNullOrWhiteSpace(category))
            return Ok(await productService.GetByCategoryAsync(category, ct));

        return Ok(await productService.GetAllAsync(ct));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductDto>> Update(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        var product = await productService.UpdateAsync(id, request, ct);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
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
public class SalesController(TransactionService transactionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SaleDto>>> GetAll(CancellationToken ct)
        => Ok(await transactionService.GetAllAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SaleDto>> GetById(Guid id, CancellationToken ct)
    {
        var sale = await transactionService.GetByIdAsync(id, ct);
        return sale is null ? NotFound() : Ok(sale);
    }

    [HttpPost]
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
