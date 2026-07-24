using System.ComponentModel.DataAnnotations;

namespace AISAM.Common.Dtos.Request
{
    public class UpdateBrandRequest
    {
        [MaxLength(255, ErrorMessage = "Name must not exceed 255 characters")]
        public string? Name { get; set; }

        [MaxLength(2000, ErrorMessage = "Description must not exceed 2000 characters")]
        public string? Description { get; set; }

        [MaxLength(500, ErrorMessage = "Logo URL must not exceed 500 characters")]
        public string? LogoUrl { get; set; }

        [MaxLength(255, ErrorMessage = "Slogan must not exceed 255 characters")]
        public string? Slogan { get; set; }

        public string? Usp { get; set; }

        public string? TargetAudience { get; set; }

        public Guid? ProfileId { get; set; }
    }
}