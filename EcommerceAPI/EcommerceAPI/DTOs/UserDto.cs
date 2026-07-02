using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace EcommerceAPI.DTOs
{
    public class UserDetailDto
    {
        /// <summary>
        /// The unique identifier for the user
        /// </summary>
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? RoleId { get; set; }
        public string? RoleName { get; set; }
        public List<string> Roles { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
    }

    public class CreateUserDto
    {
        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(150)]
        public string? FullName { get; set; }

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required, Phone, StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Username { get; set; }

        [Required, StringLength(200, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public int? RoleId { get; set; }
    }

    public class UpdateUserDto
    {
        /// <summary>
        /// The unique identifier for the user to update
        /// </summary>
        [Required]
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(150)]
        public string? FullName { get; set; }

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required, Phone, StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Username { get; set; }

        [StringLength(200, MinimumLength = 6)]
        public string? Password { get; set; }

        public bool IsActive { get; set; } = true;

        public int? RoleId { get; set; }
    }
}
