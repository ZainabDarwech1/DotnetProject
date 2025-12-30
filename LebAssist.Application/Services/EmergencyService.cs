using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using LebAssist.Application.DTOs;
using LebAssist.Application.Interfaces;

namespace LebAssist.Application.Services
{
    public class EmergencyService : IEmergencyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileStorageService _fileStorageService;

        public EmergencyService(
            IUnitOfWork unitOfWork,
            IFileStorageService fileStorageService)
        {
            _unitOfWork = unitOfWork;
            _fileStorageService = fileStorageService;
        }

        public async Task<int> CreateEmergencyAsync(int clientId, EmergencyDtos dto)
        {
            string? photoPath = null;

            if (dto.PhotoData != null && dto.PhotoFileName != null)
            {
                photoPath = await _fileStorageService.SaveFileAsync(
                    dto.PhotoData,
                    dto.PhotoFileName,
                    "emergencies");
            }

            var emergency = new EmergencyRequest
            {
                ClientId = clientId,
                ServiceId = dto.ServiceId,
                Details = dto.Description,
                Latitude = (decimal)dto.Latitude,
                Longitude = (decimal)dto.Longitude,
                Status = EmergencyStatus.Pending,
                RequestDateTime = DateTime.UtcNow
            };

            await _unitOfWork.EmergencyRequests.AddAsync(emergency);
            await _unitOfWork.SaveChangesAsync();

            return emergency.EmergencyRequestId;
        }

        public async Task<bool> AcceptEmergencyAsync(int emergencyId, int providerId)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var emergency = await _unitOfWork.EmergencyRequests.GetByIdAsync(emergencyId);

                if (emergency == null || emergency.ProviderId != null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                emergency.ProviderId = providerId;
                emergency.Status = EmergencyStatus.Accepted;
                emergency.AcceptedDateTime = DateTime.UtcNow;

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return true;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public Task<bool> DeclineEmergencyAsync(int emergencyId, int providerId)
        {
            return Task.FromResult(true);
        }

        public async Task<bool> StartEmergencyAsync(int emergencyId, int providerId)
        {
            var emergency = await _unitOfWork.EmergencyRequests.GetByIdAsync(emergencyId);

            if (emergency == null || emergency.ProviderId != providerId)
                return false;

            if (emergency.Status != EmergencyStatus.Accepted)
                return false;

            emergency.Status = EmergencyStatus.InProgress;
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> CompleteEmergencyAsync(int emergencyId, int providerId)
        {
            var emergency = await _unitOfWork.EmergencyRequests.GetByIdAsync(emergencyId);

            if (emergency == null || emergency.ProviderId != providerId)
                return false;

            if (emergency.Status != EmergencyStatus.InProgress)
                return false;

            emergency.Status = EmergencyStatus.Completed;
            emergency.CompletedDate = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<EmergencyRequest?> GetEmergencyDetailsAsync(int emergencyId)
        {
            return await _unitOfWork.EmergencyRequests.GetWithDetailsAsync(emergencyId);
        }

        public async Task<IEnumerable<EmergencyRequest>> GetClientEmergenciesAsync(int clientId)
        {
            return await _unitOfWork.EmergencyRequests.GetByClientIdAsync(clientId);
        }

        public async Task<IEnumerable<EmergencyRequest>> GetPendingEmergenciesAsync()
        {
            return await _unitOfWork.EmergencyRequests.GetPendingAsync();
        }
    }
}