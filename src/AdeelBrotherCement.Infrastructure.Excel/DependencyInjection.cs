using AdeelBrotherCement.Application.Interfaces;
using AdeelBrotherCement.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AdeelBrotherCement.Infrastructure.Excel;

public static class DependencyInjection
{
    public static IServiceCollection AddExcelInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ExcelDataOptions>(configuration.GetSection(ExcelDataOptions.SectionName));

        services.AddSingleton<ExcelWorkbookManager>();
        services.AddScoped<IProductRepository, ExcelProductRepository>();
        services.AddScoped<ITransactionRepository, ExcelTransactionRepository>();
        services.AddScoped<IExpenseRepository, ExcelExpenseRepository>();
        services.AddScoped<ICustomerRepository, ExcelCustomerRepository>();
        services.AddScoped<ICustomerPaymentRepository, ExcelCustomerPaymentRepository>();
        services.AddScoped<IStockRepository, ExcelStockRepository>();
        services.AddScoped<IUserRepository, ExcelUserRepository>();
        services.AddScoped<IDealerRepository, ExcelDealerRepository>();
        services.AddScoped<IDealerPurchaseRepository, ExcelDealerPurchaseRepository>();
        services.AddScoped<IDealerPaymentRepository, ExcelDealerPaymentRepository>();
        services.AddScoped<IAdvanceBookingRepository, ExcelAdvanceBookingRepository>();
        services.AddScoped<IShopPurchaseRepository, ExcelShopPurchaseRepository>();
        services.AddScoped<IBackupService, ExcelBackupService>();
        services.AddScoped<ISyncService, ExcelSyncService>();

        services.Configure<SyncOptions>(configuration.GetSection(SyncOptions.SectionName));

        services.AddScoped<ProductService>();
        services.AddScoped<CustomerService>();
        services.AddScoped<TransactionService>();
        services.AddScoped<ExpenseService>();
        services.AddScoped<InventoryService>();
        services.AddScoped<DashboardService>();
        services.AddScoped<UserService>();
        services.AddScoped<DealerService>();
        services.AddScoped<BookingService>();
        services.AddScoped<ReportService>();
        services.AddScoped<ShopPurchaseService>();

        return services;
    }
}
