using EcommerceAPI.DTOs;
using EcommerceAPI.Models;
using EcommerceAPI.Data;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EcommerceAPI.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<LoginResponse> RegisterAsync(RegisterRequest request);
        Task<RefreshTokenResponse> RefreshTokenAsync(string refreshToken);
        Task<bool> LogoutAsync(int userId);
        Task<string> GenerateRefreshTokenAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly EcommerceDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IPermissionService _permissionService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            EcommerceDbContext context,
            IConfiguration configuration,
            IPermissionService permissionService,
            ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _permissionService = permissionService;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .Include(u => u.RefreshTokens)
                    .FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning($"Login failed for email: {request.Email}");
                    throw new UnauthorizedAccessException("Invalid email or password.");
                }

                var passwordValid = IsPasswordValid(request.Password, user.Password);
                if (!passwordValid)
                {
                    _logger.LogWarning($"Invalid password for email: {request.Email}");
                    throw new UnauthorizedAccessException("Invalid email or password.");
                }

                var accessToken = await GenerateAccessTokenAsync(user);
                var refreshToken = await GenerateRefreshTokenAsync();

                // Store refresh token
                user.RefreshTokens.Add(new RefreshToken
                {
                    Token = refreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    UserId = user.Id
                });

                await _context.SaveChangesAsync();

                return new LoginResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error");
                throw;
            }
        }

        public async Task<LoginResponse> RegisterAsync(RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                throw new InvalidOperationException("Full name is required.");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw new InvalidOperationException("Email is required.");

            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                throw new InvalidOperationException("Phone number is required.");

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                throw new InvalidOperationException("Password must be at least 6 characters.");

            var email = request.Email.Trim();
            var phoneNumber = request.PhoneNumber.Trim();
            var exists = await _context.Users.AnyAsync(u => u.Email == email || u.Username == email);
            if (exists)
                throw new InvalidOperationException($"An account with email {email} already exists.");

            var user = new User
            {
                Name = request.FullName.Trim(),
                Email = email,
                PhoneNumber = phoneNumber,
                Username = email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return await LoginAsync(new LoginRequest { Email = email, Password = request.Password });
        }

        public async Task<RefreshTokenResponse> RefreshTokenAsync(string refreshTokenString)
        {
            try
            {
                var refreshToken = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Token == refreshTokenString);

                if (refreshToken == null || refreshToken.IsRevoked || refreshToken.ExpiryDate < DateTime.UtcNow)
                {
                    throw new UnauthorizedAccessException("Invalid or expired refresh token.");
                }

                var user = await _context.Users
                    .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                    .Include(u => u.RefreshTokens)
                    .FirstOrDefaultAsync(u => u.Id == refreshToken.UserId);

                if (user == null || !user.IsActive)
                {
                    throw new UnauthorizedAccessException("User not found or inactive.");
                }

                var accessToken = await GenerateAccessTokenAsync(user);
                var newRefreshToken = await GenerateRefreshTokenAsync();

                // Mark old token as revoked and add new one
                refreshToken.IsRevoked = true;
                user.RefreshTokens.Add(new RefreshToken
                {
                    Token = newRefreshToken,
                    ExpiryDate = DateTime.UtcNow.AddDays(7),
                    UserId = user.Id
                });

                await _context.SaveChangesAsync();

                return new RefreshTokenResponse
                {
                    AccessToken = accessToken,
                    RefreshToken = newRefreshToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refresh token error");
                throw;
            }
        }

        public async Task<bool> LogoutAsync(int userId)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                    return false;

                // Revoke all active refresh tokens
                var activeTokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                    .ToListAsync();

                foreach (var token in activeTokens)
                {
                    token.IsRevoked = true;
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout error");
                return false;
            }
        }

        public async Task<string> GenerateRefreshTokenAsync()
        {
            var randomNumber = new byte[64];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAccessTokenAsync(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? string.Empty));
            var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            var userRoles = user.UserRoles
                .Where(ur => ur.Role != null)
                .Select(ur => ur.Role.Name)
                .ToList();
            var permissions = await _permissionService.GetUserPermissionsAsync(user.Id);

            var claims = new List<Claim>
            {
                new Claim("id", user.Id.ToString()),
                new Claim("email", user.Email ?? string.Empty),
                new Claim("phoneNumber", user.PhoneNumber ?? string.Empty),
                new Claim("name", user.Name),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.Name)
            };

            foreach (var role in userRoles.DefaultIfEmpty("User"))
            {
                claims.Add(new Claim("role", role));
            }

            // Add permission claims
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission));
            }

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiryMinutes"] ?? "15")),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private bool IsPasswordValid(string password, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                return false;

            try
            {
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }
            catch (BCrypt.Net.SaltParseException ex)
            {
                _logger.LogWarning(ex, "Stored password hash is invalid");
                return false;
            }
        }
    }
}
