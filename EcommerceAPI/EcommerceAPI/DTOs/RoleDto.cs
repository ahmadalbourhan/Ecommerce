namespace EcommerceAPI.DTOs
{
    /// <summary>
    /// DTO for creating a new role
    /// </summary>
    public class CreateRoleDto
    {
        /// <summary>
        /// The name of the role
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for updating an existing role
    /// </summary>
    public class UpdateRoleDto
    {
        /// <summary>
        /// The name of the role
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// DTO for role responses
    /// </summary>
    public class RoleResponseDto
    {
        /// <summary>
        /// The role ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the role
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
