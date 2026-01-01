using Domain.Entities;
using Domain.Interfaces;
using LebAssist.Application.DTOs;
using LebAssist.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace LebAssist.Application.Services
{
    public class ClientService : IClientService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IFileStorageService _fileStorageService;
        private readonly ILogger<ClientService> _logger;

        public ClientService(
            IUnitOfWork unitOfWork,
            UserManager<IdentityUser> userManager,
            IFileStorageService fileStorageService,
            ILogger<ClientService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _fileStorageService = fileStorageService;
            _logger = logger;
        }

        public async Task<ClientRegistrationResult> RegisterAsync(ClientRegistrationDto dto)
        {
            var result = new ClientRegistrationResult();

            try
            {
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    result.Errors.Add("Email is already registered.");
                    return result;
                }

                var user = new IdentityUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    PhoneNumber = dto.PhoneNumber
                };

                var identityResult = await _userManager.CreateAsync(user, dto.Password);

                if (!identityResult.Succeeded)
                {
                    result.Errors.AddRange(identityResult.Errors.Select(e => e.Description));
                    return result;
                }

                await _userManager.AddToRoleAsync(user, "Client");

                string? profilePhotoPath = null;
                if (dto.ProfilePhotoData != null && !string.IsNullOrEmpty(dto.ProfilePhotoFileName))
                {
                    if (_fileStorageService.ValidateImageFile(dto.ProfilePhotoFileName, dto.ProfilePhotoData.Length))
                    {
                        profilePhotoPath = await _fileStorageService.SaveFileAsync(
                            dto.ProfilePhotoData,
                            dto.ProfilePhotoFileName,
                            "profiles");
                    }
                    else
                    {
                        _logger.LogWarning("Invalid profile photo file for user {Email}", dto.Email);
                    }
                }

                var client = new Client
                {
                    AspNetUserId = user.Id,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    PhoneNumber = dto.PhoneNumber,
                    Latitude = dto.Latitude,
                    Longitude = dto.Longitude,
                    ProfilePhotoPath = profilePhotoPath,
                    IsProvider = false,
                    DateRegistered = DateTime.UtcNow
                };

                await _unitOfWork.Clients.AddAsync(client);
                await _unitOfWork.SaveChangesAsync();

                result.Success = true;
                result.AspNetUserId = user.Id;
                result.ClientId = client.ClientId;

                _logger.LogInformation("Client registered successfully with ID {ClientId}", client.ClientId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering client with email {Email}", dto.Email);
                result.Errors.Add("An error occurred during registration.");
            }

            return result;
        }

        public async Task<ClientProfileDto?> GetProfileAsync(string aspNetUserId)
        {
            var client = await _unitOfWork.Clients.GetByAspNetUserIdAsync(aspNetUserId);
            if (client == null)
                return null;

            var user = await _userManager.FindByIdAsync(aspNetUserId);

            return new ClientProfileDto
            {
                ClientId = client.ClientId,
                Email = user?.Email ?? string.Empty,
                FirstName = client.FirstName,
                LastName = client.LastName,
                PhoneNumber = client.PhoneNumber,
                Latitude = client.Latitude,
                Longitude = client.Longitude,
                ProfilePhotoPath = client.ProfilePhotoPath,
                IsProvider = client.IsProvider,
                ProviderStatus = client.ProviderStatus,
                Bio = client.Bio,
                YearsOfExperience = client.YearsOfExperience,
                DateRegistered = client.DateRegistered
            };
        }

        public async Task<bool> UpdateProfileAsync(string aspNetUserId, UpdateClientProfileDto dto)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByAspNetUserIdAsync(aspNetUserId);
                if (client == null)
                    return false;

                client.FirstName = dto.FirstName;
                client.LastName = dto.LastName;
                client.PhoneNumber = dto.PhoneNumber;
                client.Latitude = dto.Latitude;
                client.Longitude = dto.Longitude;
                client.Bio = dto.Bio;
                client.YearsOfExperience = dto.YearsOfExperience;

                await _unitOfWork.Clients.UpdateAsync(client);
                await _unitOfWork.SaveChangesAsync();

                var user = await _userManager.FindByIdAsync(aspNetUserId);
                if (user != null)
                {
                    user.PhoneNumber = dto.PhoneNumber;
                    await _userManager.UpdateAsync(user);
                }

                _logger.LogInformation("Client profile updated for user {UserId}", aspNetUserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user {UserId}", aspNetUserId);
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(string aspNetUserId, string currentPassword, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(aspNetUserId);
                if (user == null)
                    return false;

                var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Password changed successfully for user {UserId}", aspNetUserId);
                    return true;
                }

                _logger.LogWarning("Password change failed for user {UserId}", aspNetUserId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", aspNetUserId);
                return false;
            }
        }

        public async Task<string?> UpdateProfilePhotoAsync(string aspNetUserId, byte[] photoData, string fileName)
        {
            try
            {
                if (!_fileStorageService.ValidateImageFile(fileName, photoData.Length))
                {
                    _logger.LogWarning("Invalid profile photo file for user {UserId}", aspNetUserId);
                    return null;
                }

                var client = await _unitOfWork.Clients.GetByAspNetUserIdAsync(aspNetUserId);
                if (client == null)
                    return null;

                if (!string.IsNullOrEmpty(client.ProfilePhotoPath))
                {
                    await _fileStorageService.DeleteFileAsync(client.ProfilePhotoPath);
                }

                var newPhotoPath = await _fileStorageService.SaveFileAsync(photoData, fileName, "profiles");
                if (newPhotoPath == null)
                    return null;

                client.ProfilePhotoPath = newPhotoPath;
                await _unitOfWork.Clients.UpdateAsync(client);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Profile photo updated for user {UserId}", aspNetUserId);
                return newPhotoPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile photo for user {UserId}", aspNetUserId);
                return null;
            }
        }

        public async Task<bool> DeleteProfilePhotoAsync(string aspNetUserId)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByAspNetUserIdAsync(aspNetUserId);
                if (client == null || string.IsNullOrEmpty(client.ProfilePhotoPath))
                    return false;

                await _fileStorageService.DeleteFileAsync(client.ProfilePhotoPath);

                client.ProfilePhotoPath = null;
                await _unitOfWork.Clients.UpdateAsync(client);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Profile photo deleted for user {UserId}", aspNetUserId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting profile photo for user {UserId}", aspNetUserId);
                return false;
            }
        }

        public async Task<ClientProfileDto?> GetClientByIdAsync(int clientId)
        {
            var client = await _unitOfWork.Clients.GetByIdAsync(clientId);
            if (client == null) return null;

            return new ClientProfileDto
            {
                ClientId = client.ClientId,
                FirstName = client.FirstName,
                LastName = client.LastName,
                PhoneNumber = client.PhoneNumber,
                Latitude = client.Latitude,
                Longitude = client.Longitude,
                ProfilePhotoPath = client.ProfilePhotoPath,
                IsProvider = client.IsProvider,
                ProviderStatus = client.ProviderStatus,
                Bio = client.Bio,
                YearsOfExperience = client.YearsOfExperience,
                DateRegistered = client.DateRegistered
            };
        }

        public async Task<ClientProfileDto?> GetClientByAspNetUserIdAsync(string aspNetUserId)
        {
            var client = await _unitOfWork.Clients.GetByAspNetUserIdAsync(aspNetUserId);
            if (client == null) return null;

            return new ClientProfileDto
            {
                ClientId = client.ClientId,
                FirstName = client.FirstName,
                LastName = client.LastName,
                PhoneNumber = client.PhoneNumber,
                Latitude = client.Latitude,
                Longitude = client.Longitude,
                ProfilePhotoPath = client.ProfilePhotoPath,
                IsProvider = client.IsProvider,
                ProviderStatus = client.ProviderStatus,
                Bio = client.Bio,
                YearsOfExperience = client.YearsOfExperience,
                DateRegistered = client.DateRegistered
            };
        }

        public async Task<IEnumerable<ClientProfileDto>> GetAllClientsAsync()
        {
            var clients = await _unitOfWork.Clients.GetAllAsync();
            var result = new List<ClientProfileDto>();

            foreach (var client in clients)
            {
                var user = await _userManager.FindByIdAsync(client.AspNetUserId);
                result.Add(new ClientProfileDto
                {
                    ClientId = client.ClientId,
                    Email = user?.Email ?? string.Empty,
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    PhoneNumber = client.PhoneNumber,
                    Latitude = client.Latitude,
                    Longitude = client.Longitude,
                    ProfilePhotoPath = client.ProfilePhotoPath,
                    IsProvider = client.IsProvider,
                    ProviderStatus = client.ProviderStatus,
                    Bio = client.Bio,
                    YearsOfExperience = client.YearsOfExperience,
                    DateRegistered = client.DateRegistered
                });
            }

            return result;
        }

        public async Task<IEnumerable<ClientProfileDto>> GetPendingProviderApplicationsAsync()
        {
            var clients = await _unitOfWork.Clients.GetAllAsync();
            var pending = clients
                .Where(c => c.ProviderStatus == Domain.Enums.ProviderStatus.Pending)
                .ToList();

            var result = new List<ClientProfileDto>();
            foreach (var client in pending)
            {
                var user = await _userManager.FindByIdAsync(client.AspNetUserId);
                result.Add(new ClientProfileDto
                {
                    ClientId = client.ClientId,
                    Email = user?.Email ?? string.Empty,
                    FirstName = client.FirstName,
                    LastName = client.LastName,
                    PhoneNumber = client.PhoneNumber,
                    Latitude = client.Latitude,
                    Longitude = client.Longitude,
                    ProfilePhotoPath = client.ProfilePhotoPath,
                    IsProvider = client.IsProvider,
                    ProviderStatus = client.ProviderStatus,
                    Bio = client.Bio,
                    YearsOfExperience = client.YearsOfExperience,
                    DateRegistered = client.DateRegistered
                });
            }

            return result;
        }
    }
}
