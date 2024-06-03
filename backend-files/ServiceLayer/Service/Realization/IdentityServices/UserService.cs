using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using DomainLayer.Models;
using ServiceLayer.DTOs;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using ServiceLayer.Services.IdentityServices;
using ServiceLayer.Service.Abstraction;

namespace ServiceLayer.Service.Realization.IdentityServices
{
    public class UserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;
        private readonly JwtTokenService _jwtTokenService;
        private readonly RoleService _roleService;
        private readonly IEarningsService _earningsService;

        public UserService(UserManager<User> userManager,
            JwtTokenService jwtTokenService,
            RoleService roleService,
            IEarningsService earningsService)
        {
            _userManager = userManager;
            MapperConfiguration configuration = new MapperConfiguration(opt =>
            {
                opt.CreateMap<User, UserDTO>();
                opt.CreateMap<UserDTO, User>();
            });
            _mapper = new Mapper(configuration);
            _jwtTokenService = jwtTokenService;
            _roleService = roleService;
            _earningsService = earningsService;
        }

        // Registers a new user with a unique ID, username, and password, and assigns a default role.
        public async Task<UserDTO> RegisterAsync(UserDTO userDto)
        {
            try
            {
                userDto.Id = Guid.NewGuid().ToString();
                var user = _mapper.Map<UserDTO, User>(userDto);
                user.UserName = Guid.NewGuid().ToString();
                var result = await _userManager.CreateAsync(user, user.PasswordHash);
                if (result.Succeeded)
                {
                    if (!_roleService.RoleExists("DefaultRole"))
                    {
                        await _roleService.AddRole("DefaultRole");
                    }
                    await _userManager.AddToRoleAsync(user, "DefaultRole");

                    var earnings = new EarningsDTO
                    {
                        UserId = user.Id,
                        TotalEarnedUSD = 0,
                        TotalEarnedBTC = 0,
                        TotalEarnedETH = 0,
                        CurrentBalanceUSD = 0,
                        CurrentBalanceBTC = 0,
                        CurrentBalanceETH = 0
                    };

                    await _earningsService.AddAsync(earnings);
                    return _mapper.Map<UserDTO>(user);
                }
                else
                {
                    throw new InvalidOperationException("Registration failed");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Registration failed");
            }
        }

        // Authenticates a user by their email and password, generating new JWT and refresh tokens if successful.
        public async Task<TokenDto> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null && await _userManager.CheckPasswordAsync(user, password))
            {
                return await _jwtTokenService.GenerateTokensAsync(user);
            }
            return null;
        }

        // Renews JWT and refresh tokens using an expired JWT and a current valid refresh token.
        public async Task<TokenDto> RenewTokenAsync(string expiredToken, string refreshToken)
        {
            var principal = _jwtTokenService.GetPrincipalFromExpiredToken(expiredToken);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && user.RefreshToken == refreshToken && user.RefreshTokenExpiryTime > DateTime.Now)
            {
                return await _jwtTokenService.RenewTokensAsync(refreshToken);
            }
            throw new SecurityTokenException("Invalid token or refresh token");
        }

        // Validates a JWT by extracting its claims and comparing the stored token version with the current version in the database.
        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var principal = _jwtTokenService.GetPrincipalFromExpiredToken(token);
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var tokenVersion = principal.FindFirst("TokenVersion")?.Value;

                var user = await _userManager.FindByIdAsync(userId);
                return user != null && !user.LockoutEnabled && user.TokenVersion.ToString() == tokenVersion;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> LogoutAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.RefreshToken = "";
                user.RefreshTokenExpiryTime = DateTime.UtcNow;
                user.TokenVersion++;
                user.RefreshTokenVersion++;
                var result = await _userManager.UpdateAsync(user);

                return result.Succeeded;
            }
            return false;
        }

        // Retrieves the remaining valid session time from an existing JWT.
        public async Task<TimeSpan?> GetRemainingSessionTimeAsync(string token)
        {
            try
            {
                var principal = _jwtTokenService.GetPrincipalFromExpiredToken(token);
                var expClaim = principal.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
                if (expClaim != null && long.TryParse(expClaim, out var expValue))
                {
                    var expiryDate = DateTimeOffset.FromUnixTimeSeconds(expValue).DateTime;
                    if (expiryDate > DateTime.UtcNow)
                    {
                        return expiryDate - DateTime.UtcNow;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // Fetches user details based on the claims principal derived from the current session context.
        public async Task<UserDTO> GetUserByPrincipalAsync(ClaimsPrincipal principal)
        {
            var user = await _userManager.GetUserAsync(principal);
            return _mapper.Map<User, UserDTO>(user);
        }

        // Allows a user to change their password after verifying their current credentials.
        public async Task<IdentityResult> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        // Retrieves a user's details based on their ID.
        public async Task<UserDTO> FindByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return _mapper.Map<User, UserDTO>(user);
        }

        // Retrieves user information using a valid JWT, primarily to extract identity from a token.
        public async Task<UserDTO> GetUserFromToken(string token)
        {
            var principal = _jwtTokenService.GetPrincipalFromExpiredToken(token);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                return _mapper.Map<User, UserDTO>(user);
            }
            throw new InvalidOperationException("User not found");
        }

        // Finds a user by their email address.
        public async Task<UserDTO> FindByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return _mapper.Map<User, UserDTO>(user);
        }

        // Retrieves the ID for a user based on their claims principal.
        public string GetUserId(ClaimsPrincipal claims)
        {
            return _userManager.GetUserId(claims);
        }

        // Resets a user's password using a secure token-based mechanism if the original password is considered compromised or forgotten.
        public async Task<IdentityResult> RestorePassword(string email, string newPassword)
        {
            User user = await _userManager.FindByEmailAsync(email);
            PasswordHasher<User> hasher = new PasswordHasher<User>();
            PasswordVerificationResult res = hasher.VerifyHashedPassword(user, user.PasswordHash, newPassword);
            if (res == PasswordVerificationResult.Failed)
            {
                string resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var identityRes = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
                return identityRes;
            }
            IdentityError error = new IdentityError();
            error.Code = "OldPasswordMustNotMatch";
            return IdentityResult.Failed(error);
        }

        // Adds a user to a specified role.
        public async Task<IdentityResult> AddToRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return await _userManager.AddToRoleAsync(user, role);
        }

        // Retrieves all users from the system.
        public List<UserDTO> GetAllUsers()
        {
            var users = _userManager.Users.ToList();
            return users.Select(user => _mapper.Map<UserDTO>(user)).ToList();
        }

        // Retrieves the primary role of a user.
        public async Task<string> GetUserRoleAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            var roles = await _userManager.GetRolesAsync(user);
            return roles.FirstOrDefault();
        }

        // Removes a user from a specified role.
        public async Task<IdentityResult> RemoveFromRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            return await _userManager.RemoveFromRoleAsync(user, role);
        }

        // Validates a password against the configured password validation rules.
        public async Task<List<IdentityResult>> ValidatePassword(string password)
        {
            return (await Task.WhenAll(_userManager.PasswordValidators.Select(validator => validator.ValidateAsync(_userManager, null, password)))).ToList();
        }

        // Invalidates all tokens for a user by incrementing the token version, forcing a logout across all sessions.
        public async Task InvalidateAllTokensAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.TokenVersion++;
                await _userManager.UpdateAsync(user);
            }
        }

        public async Task<IdentityResult> UpdateUserProfileAsync(UserDTO userDto)
        {
            var user = await _userManager.FindByIdAsync(userDto.Id);
            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // Map the DTO to the User domain model
            _mapper.Map(userDto, user);

            // Update user details
            var result = await _userManager.UpdateAsync(user);
            return result;
        }
    }
}
