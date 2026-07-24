using System.ComponentModel.DataAnnotations;
using AISAM.Data.Enumeration;
using Microsoft.AspNetCore.Http;

namespace AISAM.Common.Dtos.Request
{
    public class UpdateProfileRequest
    {
        [MaxLength(255, ErrorMessage = "Name must not exceed 255 characters")]
        public string? Name { get; set; }

        /// <summary>
        /// Profile type (Free, Basic, Pro)
        /// </summary>
        public ProfileTypeEnum? ProfileType { get; set; }

        [MaxLength(255, ErrorMessage = "Company name must not exceed 255 characters")]
        public string? CompanyName { get; set; }

        [MaxLength(1000, ErrorMessage = "Bio must not exceed 1000 characters")]
        public string? Bio { get; set; }

        [MaxLength(500, ErrorMessage = "Avatar URL must not exceed 500 characters")]
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// Avatar file to upload (alternative to AvatarUrl)
        /// </summary>
        public IFormFile? AvatarFile { get; set; }
    }
}
