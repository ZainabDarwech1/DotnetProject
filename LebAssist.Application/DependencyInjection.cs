using FluentValidation;
using LebAssist.Application.Interfaces;
using LebAssist.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LebAssist.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IServiceService, ServiceService>();
            services.AddScoped<IProviderService, ProviderService>();
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IEmergencyService, EmergencyService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IDashboardService, DashboardService>();
            services.AddScoped<IProviderDashboardService, ProviderDashboardService>();
            return services;
        }
    }
}