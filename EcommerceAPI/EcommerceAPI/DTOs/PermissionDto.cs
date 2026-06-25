using System.ComponentModel.DataAnnotations;

namespace EcommerceAPI.DTOs
{
    public class PermissionDto
    {
        /// <summary>
        /// The unique identifier for the permission
        /// </summary>
        public int Id { get; set; }
        public string Slug { get; set; } = string.Empty;
    }

    public class CreatePermissionDto
    {
        [Required, StringLength(200)]
        public string Slug { get; set; } = string.Empty;
    }

    public class UpdatePermissionDto
    {
        /// <summary>
        /// The unique identifier for the permission to update
        /// </summary>
        [Required]
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Slug { get; set; } = string.Empty;
    }
}
