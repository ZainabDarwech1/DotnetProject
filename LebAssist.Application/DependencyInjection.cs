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

            // R7 & R8
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<IEmergencyService, EmergencyService>();

            // R12: Notifications
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IReviewService, ReviewService>();
           
            return services;
        }
    }
}