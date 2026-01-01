using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace LebAssist.Application.DTOs
{
    public class ClientRegistrationDto
    {
        [Required]
        [EmailAddress]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Phone]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Range(-90, 90)]
        public decimal Latitude { get; set; }

        [Range(-180, 180)]
        public decimal Longitude { get; set; }

        public byte[]? ProfilePhotoData { get; set; }
        public string? ProfilePhotoFileName { get; set; }
    }

    public class ClientRegistrationResult
    {
        public bool Success { get; set; }
        public string? AspNetUserId { get; set; }
        public int? ClientId { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    public class ClientProfileDto
    {
        public int ClientId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string? ProfilePhotoPath { get; set; }
        public bool IsProvider { get; set; }
        public ProviderStatus? ProviderStatus { get; set; }
        public string? Bio { get; set; }
        public int? YearsOfExperience { get; set; }
        public DateTime DateRegistered { get; set; }
    }

    public class UpdateClientProfileDto
    {
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Phone]
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Range(-90, 90)]
        public decimal Latitude { get; set; }

        [Range(-180, 180)]
        public decimal Longitude { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }

        [Range(0, 100)]
        public int? YearsOfExperience { get; set; }
    }
}
