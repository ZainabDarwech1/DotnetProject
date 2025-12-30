using Domain.Interfaces;
using LebAssist.Application.Interfaces;
using LebAssist.Infrastructure.Data;
using LebAssist.Infrastructure.Repositories;
using LebAssist.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LebAssist.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IFileStorageService, LocalFileStorageService>();

            // R12: Email Service
            services.AddScoped<IEmailService, EmailService>();

            return services;
        }
    }
}