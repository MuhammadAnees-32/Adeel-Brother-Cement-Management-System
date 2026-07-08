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

        services.AddScoped<ProductService>();
        services.AddScoped<CustomerService>();
        services.AddScoped<TransactionService>();
        services.AddScoped<ExpenseService>();
        services.AddScoped<InventoryService>();
        services.AddScoped<DashboardService>();

        return services;
    }
}
