using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using LebAssist.Application.DTOs;
using LebAssist.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace LebAssist.Application.Services
{
    public partial class ProviderService : IProviderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ProviderService> _logger;

        public ProviderService(
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorageService,
            UserManager<IdentityUser> userManager,
            ILogger<ProviderService> logger)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
            _userManager = userManager;
            _logger = logger;
        }

        // ================================
        // PROVIDER APPLICATION
        // ================================

        public async Task<bool> ApplyAsProviderAsync(string userId, ProviderApplicationDto dto)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByAspNetUserIdAsync(userId);
                if (client == null)
                {
                    _logger.LogWarning("Client not found for user {UserId}", userId);
                    return false;
                }

                // Check if already a provider
                if (client.IsProvider)
                {
                    _logger.LogWarning("User {UserId} is already a provider", userId);
                    return false;
                }

                // Update client with provider info
                client.Bio = dto.Bio;
                client.YearsOfExperience = dto.YearsOfExperience;
                // client.IsProvider = true;
                client.ProviderStatus = ProviderStatus.Pending; // Pending approval

                await _unitOfWork.Clients.UpdateAsync(client);

                // Add selected services
                foreach (var serviceId in dto.SelectedServiceIds)
                {
                    var service = await _unitOfWork.Services.GetByIdAsync(serviceId);
                    if (service != null)
                    {
                        // Add ProviderService (inactive until approved)
                        var providerService = new ProviderServiceEntity
                        {
                            ClientId = client.ClientId,
                            ServiceId = serviceId,
                            IsActive = false, // Not active until approved
                            DateAdded = DateTime.UtcNow,
                            PricePerHour = 0 // Provider will set price later
                        };

                        await _unitOfWork.ProviderServices.AddAsync(providerService);

                        // Add working hours for this service
                        var serviceWorkingHours = dto.ServiceWorkingHours
                            .FirstOrDefault(swh => swh.ServiceId == serviceId);

                        if (serviceWorkingHours != null && serviceWorkingHours.DaySchedules.Any())
                        {
                            foreach (var daySchedule in serviceWorkingHours.DaySchedules)
                            {
                                var workingHour = new ProviderWorkingHours
                                {
                                    ClientId = client.ClientId,
                                    ServiceId = serviceId,
                                    DayOfWeek = (DayOfWeekEnum)daySchedule.DayOfWeek,
                                    StartTime = daySchedule.StartTime,
                                    EndTime = daySchedule.EndTime,
                                    IsActive = false // Not active until approved
                                };

                                await _unitOfWork.ProviderWorkingHours.AddAsync(workingHour);
                            }
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                // Create notification for all admins
                await NotifyAdminsOfProviderApplicationAsync(client);

                _logger.LogInformation("Provider application submitted for user {UserId}, ClientId {ClientId}", userId, client.ClientId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting provider application for user {UserId}", userId);
                return false;
            }
        }

        private async Task NotifyAdminsOfProviderApplicationAsync(Client client)
        {
            try
            {
                // Get all admin users (you may need to adjust this based on your role management)
                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");

                foreach (var adminUser in adminUsers)
                {
                    var notification = new Notification
                    {
                        UserId = adminUser.Id,
                        Type = NotificationType.ProviderApplication,
                        ReferenceId = client.ClientId,
                        Title = "New Provider Application",
                        Message = $"{client.FirstName} {client.LastName} has applied to become a service provider.",
                        IsRead = false,
                        CreatedDate = DateTime.UtcNow
                    };

                    await _unitOfWork.Notifications.AddAsync(notification);
                }

                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("Admin notifications created for provider application from client {ClientId}", client.ClientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying admins of provider application for client {ClientId}", client.ClientId);
                // Log but don't fail the entire application - notifications are secondary
            }
        }

        // ================================
        // EXISTING METHODS
        // ================================

        public async Task<IEnumerable<ProviderServiceDto>> GetProviderServicesAsync(int clientId)
        {
            var providerServices = await _unitOfWork.ProviderServices.GetAllByProviderIdAsync(clientId);

            return providerServices
                .Select(ps => new ProviderServiceDto
                {
                    ProviderServiceId = ps.ProviderServiceId,
                    ServiceId = ps.ServiceId,
                    ServiceName = ps.Service?.ServiceName ?? "Unknown",
                    CategoryName = ps.Service?.Category?.CategoryName ?? "Unknown",
                    IsActive = ps.IsActive,
                    PricePerHour = ps.PricePerHour ?? 0,
                    DateAdded = ps.DateAdded
                });
        }

        public async Task<bool> AddProviderServiceAsync(int clientId, AddProviderServiceDto dto)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
                if (client == null || !client.IsProvider)
                {
                    _logger.LogWarning("Client {ClientId} is not a provider", clientId);
                    return false;
                }

                var service = await _unitOfWork.Services.GetByIdAsync(dto.ServiceId);
                if (service == null)
                {
                    _logger.LogWarning("Service {ServiceId} not found", dto.ServiceId);
                    return false;
                }

                var existingServices = await _unitOfWork.ProviderServices.GetAllAsync();
                if (existingServices.Any(ps => ps.ClientId == clientId && ps.ServiceId == dto.ServiceId))
                {
                    _logger.LogWarning("Provider {ClientId} already offers service {ServiceId}", clientId, dto.ServiceId);
                    return false;
                }

                var providerService = new ProviderServiceEntity
                {
                    ClientId = clientId,
                    ServiceId = dto.ServiceId,
                    IsActive = true,
                    DateAdded = DateTime.UtcNow,
                    PricePerHour = dto.PricePerHour
                };

                await _unitOfWork.ProviderServices.AddAsync(providerService);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Provider service added for client {ClientId}, service {ServiceId}", clientId, dto.ServiceId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding provider service for client {ClientId}", clientId);
                return false;
            }
        }

        public async Task<bool> AddProviderServiceWithHoursAsync(int clientId, AddProviderServiceDto dto, List<ProviderWorkingHoursDto> workingHours)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
                if (client == null || !client.IsProvider)
                {
                    _logger.LogWarning("Client {ClientId} is not a provider", clientId);
                    return false;
                }

                var service = await _unitOfWork.Services.GetByIdAsync(dto.ServiceId);
                if (service == null)
                {
                    _logger.LogWarning("Service {ServiceId} not found", dto.ServiceId);
                    return false;
                }

                var existingServices = await _unitOfWork.ProviderServices.GetAllAsync();
                if (existingServices.Any(ps => ps.ClientId == clientId && ps.ServiceId == dto.ServiceId))
                {
                    _logger.LogWarning("Provider {ClientId} already offers service {ServiceId}", clientId, dto.ServiceId);
                    return false;
                }

                var providerService = new ProviderServiceEntity
                {
                    ClientId = clientId,
                    ServiceId = dto.ServiceId,
                    IsActive = true,
                    DateAdded = DateTime.UtcNow,
                    PricePerHour = dto.PricePerHour
                };

                await _unitOfWork.ProviderServices.AddAsync(providerService);
                await _unitOfWork.SaveChangesAsync();

                // Add working hours for this service
                if (workingHours != null && workingHours.Any())
                {
                    var serviceHours = workingHours.FirstOrDefault(wh => wh.ServiceId == dto.ServiceId);
                    if (serviceHours != null && serviceHours.DaySchedules.Any())
                    {
                        foreach (var daySchedule in serviceHours.DaySchedules)
                        {
                            var workingHour = new ProviderWorkingHours
                            {
                                ClientId = clientId,
                                ServiceId = dto.ServiceId,
                                DayOfWeek = (DayOfWeekEnum)daySchedule.DayOfWeek,
                                StartTime = daySchedule.StartTime,
                                EndTime = daySchedule.EndTime,
                                IsActive = true
                            };

                            await _unitOfWork.ProviderWorkingHours.AddAsync(workingHour);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Provider service with hours added for client {ClientId}, service {ServiceId}", clientId, dto.ServiceId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding provider service with hours for client {ClientId}", clientId);
                return false;
            }
        }

        public async Task<bool> RemoveProviderServiceAsync(int clientId, int serviceId)
        {
            try
            {
                var providerServices = await _unitOfWork.ProviderServices.GetAllAsync();
                var providerService = providerServices.FirstOrDefault(ps => ps.ClientId == clientId && ps.ServiceId == serviceId);

                if (providerService == null)
                    return false;

                var bookings = await _unitOfWork.Bookings.GetAllAsync();
                var hasActiveBookings = bookings.Any(b =>
                    b.ProviderId == clientId &&
                    b.ServiceId == serviceId &&
                    (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Accepted || b.Status == BookingStatus.InProgress));

                if (hasActiveBookings)
                {
                    _logger.LogWarning("Cannot remove service {ServiceId} for provider {ClientId} - active bookings exist", serviceId, clientId);
                    return false;
                }

                await _unitOfWork.ProviderServices.DeleteAsync(providerService.ProviderServiceId);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Provider service removed for client {ClientId}, service {ServiceId}", clientId, serviceId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing provider service for client {ClientId}", clientId);
                return false;
            }
        }

        public async Task<bool> UpdateProviderServicePriceAsync(int clientId, int serviceId, decimal pricePerHour)
        {
            try
            {
                var providerServices = await _unitOfWork.ProviderServices.GetAllAsync();
                var providerService = providerServices.FirstOrDefault(ps => ps.ClientId == clientId && ps.ServiceId == serviceId);

                if (providerService == null)
                    return false;

                providerService.PricePerHour = pricePerHour;
                await _unitOfWork.ProviderServices.UpdateAsync(providerService);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Provider service price updated for client {ClientId}, service {ServiceId}", clientId, serviceId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating provider service price for client {ClientId}", clientId);
                return false;
            }
        }

        public async Task<IEnumerable<PortfolioPhotoDto>> GetProviderPortfolioAsync(int clientId)
        {
            var portfolios = await _unitOfWork.Repository<ProviderPortfolio>().GetAllAsync();

            return portfolios
                .Where(p => p.ClientId == clientId)
                .OrderBy(p => p.DisplayOrder)
                .ThenByDescending(p => p.UploadDate)
                .Select(p => new PortfolioPhotoDto
                {
                    PortfolioPhotoId = p.PortfolioPhotoId,
                    PhotoPath = p.PhotoPath,
                    Caption = p.Caption,
                    UploadDate = p.UploadDate,
                    DisplayOrder = p.DisplayOrder
                });
        }

        public async Task<int> AddPortfolioPhotoAsync(int clientId, byte[] photoData, string fileName, string? caption)
        {
            try
            {
                if (!_fileStorageService.ValidateImageFile(fileName, photoData.Length))
                {
                    _logger.LogWarning("Invalid portfolio photo file for client {ClientId}", clientId);
                    return 0;
                }

                var photoPath = await _fileStorageService.SaveFileAsync(photoData, fileName, "portfolios");
                if (photoPath == null)
                    return 0;

                var portfolios = await _unitOfWork.Repository<ProviderPortfolio>().GetAllAsync();
                var maxOrder = portfolios.Where(p => p.ClientId == clientId).Max(p => (int?)p.DisplayOrder) ?? 0;

                var portfolio = new ProviderPortfolio
                {
                    ClientId = clientId,
                    PhotoPath = photoPath,
                    Caption = caption,
                    UploadDate = DateTime.UtcNow,
                    DisplayOrder = maxOrder + 1
                };

                await _unitOfWork.Repository<ProviderPortfolio>().AddAsync(portfolio);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Portfolio photo added for client {ClientId}", clientId);
                return portfolio.PortfolioPhotoId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding portfolio photo for client {ClientId}", clientId);
                return 0;
            }
        }

        public async Task<bool> DeletePortfolioPhotoAsync(int clientId, int portfolioPhotoId)
        {
            try
            {
                var portfolio = await _unitOfWork.Repository<ProviderPortfolio>().GetByIdAsync(portfolioPhotoId);
                if (portfolio == null || portfolio.ClientId != clientId)
                    return false;

                await _fileStorageService.DeleteFileAsync(portfolio.PhotoPath);

                await _unitOfWork.Repository<ProviderPortfolio>().DeleteAsync(portfolioPhotoId);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Portfolio photo {PhotoId} deleted for client {ClientId}", portfolioPhotoId, clientId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting portfolio photo {PhotoId} for client {ClientId}", portfolioPhotoId, clientId);
                return false;
            }
        }

        public async Task<bool> ReorderPortfolioPhotosAsync(int clientId, List<int> photoIds)
        {
            try
            {
                var portfolios = await _unitOfWork.Repository<ProviderPortfolio>().GetAllAsync();
                var clientPortfolios = portfolios.Where(p => p.ClientId == clientId).ToList();

                for (int i = 0; i < photoIds.Count; i++)
                {
                    var portfolio = clientPortfolios.FirstOrDefault(p => p.PortfolioPhotoId == photoIds[i]);
                    if (portfolio != null)
                    {
                        portfolio.DisplayOrder = i + 1;
                        await _unitOfWork.Repository<ProviderPortfolio>().UpdateAsync(portfolio);
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Portfolio photos reordered for client {ClientId}", clientId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering portfolio photos for client {ClientId}", clientId);
                return false;
            }
        }
        public async Task<bool> IsProviderAvailableAsync(int providerId)
        {
            return await _unitOfWork.Clients.IsProviderAvailableAsync(providerId);
        }

        // ================================
        // NEWLY ADDED METHODS
        // ================================

        public async Task<string?> SaveProfilePhotoAsync(string userId, byte[] photoData, string fileName)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByAspNetUserIdAsync(userId);
                if (client == null)
                {
                    _logger.LogWarning("Client not found for user {UserId}", userId);
                    return null;
                }

                if (!_fileStorageService.ValidateImageFile(fileName, photoData.Length))
                {
                    _logger.LogWarning("Invalid profile photo file for user {UserId}", userId);
                    return null;
                }

                var photoPath = await _fileStorageService.SaveFileAsync(photoData, fileName, "profiles");
                if (photoPath == null)
                    return null;

                // Delete old profile photo if exists
                if (!string.IsNullOrEmpty(client.ProfilePhotoPath))
                {
                    await _fileStorageService.DeleteFileAsync(client.ProfilePhotoPath);
                }

                // Update client with new profile photo
                client.ProfilePhotoPath = photoPath;
                await _unitOfWork.Clients.UpdateAsync(client);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Profile photo saved for user {UserId}", userId);
                return photoPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving profile photo for user {UserId}", userId);
                return null;
            }
        }

        public async Task<IEnumerable<ProviderWorkingHoursDto>> GetProviderWorkingHoursAsync(int clientId)
        {
            var hours = await _unitOfWork.ProviderWorkingHours.GetByProviderIdAsync(clientId);

            // Group by service so the DTO contains all day schedules for a service
            var grouped = hours
                .GroupBy(h => h.ServiceId)
                .Select(g => new ProviderWorkingHoursDto
                {
                    ServiceId = g.Key,
                    DaySchedules = g
                        .OrderBy(h => h.DayOfWeek)
                        .Select(h => new DayScheduleDto
                        {
                            DayOfWeek = (int)h.DayOfWeek,
                            StartTime = h.StartTime,
                            EndTime = h.EndTime
                        }).ToList()
                });

            return grouped;
        }

        public async Task<bool> ApproveProviderAsync(int clientId, string adminUserId)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
                if (client == null) return false;

                // Activate provider flag and status
                client.IsProvider = true;
                client.ProviderStatus = ProviderStatus.Approved;
                await _unitOfWork.Clients.UpdateAsync(client);

                // Activate provider services
                var services = await _unitOfWork.ProviderServices.GetAllAsync();
                var clientServices = services.Where(s => s.ClientId == clientId).ToList();
                foreach (var ps in clientServices)
                {
                    ps.IsActive = true;
                    await _unitOfWork.ProviderServices.UpdateAsync(ps);
                }

                // Activate working hours
                var hours = await _unitOfWork.ProviderWorkingHours.GetByProviderIdAsync(clientId);
                foreach (var h in hours)
                {
                    h.IsActive = true;
                    await _unitOfWork.ProviderWorkingHours.UpdateAsync(h);
                }

                // Add Provider role to user
                var user = await _userManager.FindByIdAsync(client.AspNetUserId);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (!roles.Contains("Provider"))
                    {
                        await _userManager.AddToRoleAsync(user, "Provider");
                    }

                    // Send approval email via email service if available
                    // Try get IEmailService via service provider - but currently not injected here
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Provider approved by admin {AdminId} for client {ClientId}", adminUserId, clientId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving provider {ClientId}", clientId);
                return false;
            }
        }

        public async Task<bool> RejectProviderAsync(int clientId, string reason, string adminUserId)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
                if (client == null) return false;

                client.ProviderStatus = ProviderStatus.Rejected;
                await _unitOfWork.Clients.UpdateAsync(client);

                // Keep provider services and working hours as inactive (IsActive = false)

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Provider application rejected by admin {AdminId} for client {ClientId}. Reason: {Reason}", adminUserId, clientId, reason);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting provider {ClientId}", clientId);
                return false;
            }
        }

        public async Task<bool> UpdateProviderServiceAsync(int clientId, int serviceId, decimal pricePerHour, List<ProviderWorkingHoursDto> workingHours)
        {
            try
            {
                var providerServices = await _unitOfWork.ProviderServices.GetAllAsync();
                var providerService = providerServices.FirstOrDefault(ps => ps.ClientId == clientId && ps.ServiceId == serviceId);

                if (providerService == null)
                    return false;

                // Update price
                providerService.PricePerHour = pricePerHour;
                await _unitOfWork.ProviderServices.UpdateAsync(providerService);

                // Delete existing working hours for this service
                var existingHours = await _unitOfWork.ProviderWorkingHours.GetByProviderAndServiceAsync(clientId, serviceId);
                foreach (var hour in existingHours)
                {
                    await _unitOfWork.ProviderWorkingHours.DeleteAsync(hour.WorkingHourId);
                }

                // Add new working hours
                if (workingHours != null && workingHours.Any())
                {
                    var serviceHours = workingHours.FirstOrDefault(wh => wh.ServiceId == serviceId);
                    if (serviceHours != null && serviceHours.DaySchedules.Any())
                    {
                        foreach (var daySchedule in serviceHours.DaySchedules)
                        {
                            var workingHour = new ProviderWorkingHours
                            {
                                ClientId = clientId,
                                ServiceId = serviceId,
                                DayOfWeek = (DayOfWeekEnum)daySchedule.DayOfWeek,
                                StartTime = daySchedule.StartTime,
                                EndTime = daySchedule.EndTime,
                                IsActive = true
                            };

                            await _unitOfWork.ProviderWorkingHours.AddAsync(workingHour);
                        }
                    }
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Provider service updated for client {ClientId}, service {ServiceId}", clientId, serviceId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating provider service for client {ClientId}", clientId);
                return false;
            }
        }

        public async Task<IEnumerable<ProviderWorkingHoursDto>> GetServiceWorkingHoursAsync(int clientId, int serviceId)
        {
            var hours = await _unitOfWork.ProviderWorkingHours.GetByProviderAndServiceAsync(clientId, serviceId);
            
            if (!hours.Any())
                return new List<ProviderWorkingHoursDto>();

            return new List<ProviderWorkingHoursDto>
            {
                new ProviderWorkingHoursDto
                {
                    ServiceId = serviceId,
                    DaySchedules = hours
                        .OrderBy(h => h.DayOfWeek)
                        .Select(h => new DayScheduleDto
                        {
                            DayOfWeek = (int)h.DayOfWeek,
                            StartTime = h.StartTime,
                            EndTime = h.EndTime
                        }).ToList()
                }
            };
        }

        public async Task<IEnumerable<ProviderOfferingDto>> GetProvidersByServiceIdAsync(int serviceId)
        {
            var providerServices = await _unitOfWork.ProviderServices.GetByServiceIdAsync(serviceId);
            return providerServices.Select(ps => new ProviderOfferingDto
            {
                ProviderId = ps.ClientId,
                ProviderName = ps.Provider != null ? $"{ps.Provider.FirstName} {ps.Provider.LastName}" : "Unknown",
                ProviderPhotoPath = ps.Provider?.ProfilePhotoPath,
                ProviderServiceId = ps.ProviderServiceId,
                PricePerHour = ps.PricePerHour ?? 0
            });
        }
    }
}