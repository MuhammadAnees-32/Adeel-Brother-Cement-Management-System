using AdeelBrotherCement.Application.DTOs;
using AdeelBrotherCement.Application.Interfaces;
using AdeelBrotherCement.Domain.Entities;

namespace AdeelBrotherCement.Application.Services;

public class BookingService(
    IAdvanceBookingRepository bookingRepository,
    IProductRepository productRepository,
    CustomerService customerService,
    TransactionService transactionService)
{
    public async Task<IReadOnlyList<AdvanceBookingDto>> GetAllAsync(CancellationToken ct = default)
    {
        var bookings = await bookingRepository.GetAllAsync(ct);
        return bookings.Select(Map).OrderByDescending(b => b.BookedDate).ToList();
    }

    public async Task<AdvanceBookingDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var booking = await bookingRepository.GetByIdAsync(id, ct);
        return booking is null ? null : Map(booking);
    }

    public async Task<AdvanceBookingDto> CreateAsync(CreateAdvanceBookingRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
            throw new InvalidOperationException("Customer name is required.");

        if (string.IsNullOrWhiteSpace(request.CustomerMobile))
            throw new InvalidOperationException("Customer mobile is required.");

        if (request.Quantity <= 0)
            throw new InvalidOperationException("Quantity must be greater than zero.");

        var product = await productRepository.GetByIdAsync(request.ProductId, ct)
            ?? throw new InvalidOperationException("Product not found.");

        var totalAmount = request.Quantity * request.UnitPrice;

        if (request.AdvancePaid < 0)
            throw new InvalidOperationException("Advance payment cannot be negative.");

        if (request.AdvancePaid > totalAmount)
            throw new InvalidOperationException("Advance payment cannot exceed total amount.");

        var customer = await customerService.FindOrCreateAsync(
            request.CustomerName, request.CustomerMobile, ct);

        var booking = new AdvanceBooking
        {
            Id = Guid.NewGuid(),
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            CustomerMobile = CustomerService.NormalizePhone(request.CustomerMobile),
            ProductId = product.Id,
            ProductName = product.Name,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            TotalAmount = totalAmount,
            AdvancePaid = request.AdvancePaid,
            RemainingAmount = totalAmount - request.AdvancePaid,
            DeliveryDate = request.DeliveryDate,
            BookedDate = DateTime.Now,
            Status = BookingStatus.Pending,
            Notes = request.Notes?.Trim()
        };

        var created = await bookingRepository.CreateAsync(booking, ct);
        return Map(created);
    }

    public async Task<SaleDto> DeliverAsync(Guid bookingId, decimal? amountPaid, CancellationToken ct = default)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId, ct)
            ?? throw new InvalidOperationException("Booking not found.");

        if (booking.Status != BookingStatus.Pending)
            throw new InvalidOperationException("Only pending bookings can be delivered.");

        var product = await productRepository.GetByIdAsync(booking.ProductId, ct)
            ?? throw new InvalidOperationException("Product not found.");

        if (product.StockQuantity < booking.Quantity)
            throw new InvalidOperationException(
                $"Insufficient stock for {product.Name}. Available: {product.StockQuantity} {product.Unit}.");

        var paid = amountPaid ?? booking.TotalAmount;
        if (paid < booking.AdvancePaid)
            throw new InvalidOperationException("Total paid cannot be less than advance already received.");

        var sale = await transactionService.CreateAsync(new CreateSaleRequest(
            booking.CustomerName,
            booking.CustomerMobile,
            booking.CustomerId,
            DateTime.Now,
            paid,
            $"Delivered from advance booking {booking.Id}",
            [new SaleItemRequest(booking.ProductId, booking.Quantity, booking.UnitPrice)]), ct);

        booking.Status = BookingStatus.Delivered;
        booking.InvoiceId = sale.Id;
        booking.RemainingAmount = 0;
        await bookingRepository.UpdateAsync(booking, ct);

        return sale;
    }

    private static AdvanceBookingDto Map(AdvanceBooking b) => new(
        b.Id, b.CustomerId, b.CustomerName, b.CustomerMobile,
        b.ProductId, b.ProductName, b.Quantity, b.UnitPrice,
        b.TotalAmount, b.AdvancePaid, b.RemainingAmount,
        b.DeliveryDate, b.BookedDate, b.Status.ToString(),
        b.InvoiceId, b.Notes);
}
