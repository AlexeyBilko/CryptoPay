using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using DomainLayer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using ServiceLayer.DTOs;

namespace ServiceLayer.Service.Realization.IdentityServices
{
    public class JwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;

        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public JwtTokenService(IConfiguration configuration, UserManager<User> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;

            _secretKey = _configuration["JwtSettings:SecretKey"];
            _issuer = _configuration["JwtSettings:Issuer"];
            _audience = _configuration["JwtSettings:Audience"];
        }

        // Generates a JWT for a user with claims including username, ID, and token version.
        public string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("TokenVersion", user.TokenVersion.ToString()) // Adding token version as a claim
            };

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse("180")), // Set duration in appsettings
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Generates both a new JWT and a refresh token for a user, storing the refresh token with an expiry date.
        public async Task<TokenDto> GenerateTokensAsync(User user)
        {
            string jwtToken = GenerateToken(user);
            string refreshToken = GenerateRefreshToken();
            await StoreRefreshToken(user, refreshToken);

            return new TokenDto { JwtToken = jwtToken, RefreshToken = refreshToken };
        }

        // Generates a cryptographically secure random refresh token.
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        // Stores a new refresh token and its expiry time in the user's record, also updates the token version.
        private async Task StoreRefreshToken(User user, string refreshToken)
        {
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
            user.RefreshTokenVersion++;
            await _userManager.UpdateAsync(user);
        }

        // Renews both JWT and refresh tokens if the provided refresh token is valid and not expired.
        public async Task<TokenDto> RenewTokensAsync(string refreshToken)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken && u.RefreshTokenExpiryTime > DateTime.Now);
            if (user == null)
                throw new SecurityTokenException("Invalid or expired refresh token");

            if (DateTime.Now > user.RefreshTokenExpiryTime)
                throw new SecurityTokenExpiredException("Refresh token has expired and cannot be renewed.");

            string newJwtToken = GenerateToken(user);
            string newRefreshToken = GenerateRefreshToken();

            await StoreRefreshToken(user, newRefreshToken);

            return new TokenDto { JwtToken = newJwtToken, RefreshToken = newRefreshToken };
        }

        // Extracts the claims principal from a JWT even if it has expired, to allow token refresh scenarios.
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                SecurityToken securityToken;
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
                JwtSecurityToken jwtSecurityToken = securityToken as JwtSecurityToken;

                if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    throw new SecurityTokenValidationException("The token is invalid or has been tampered with.");
                }

                return principal;
            }
            catch (SecurityTokenExpiredException)
            {
                throw new SecurityTokenExpiredException("Token has expired.");
            }
            catch (SecurityTokenValidationException ex)
            {
                throw new SecurityTokenValidationException("Token validation failed.", ex);
            }
        }

        // Validates a token's integrity and checks against the current token version stored in the user's record.
        public async Task<bool> IsTokenValid(string token)
        {
            try
            {
                var principal = GetPrincipalFromExpiredToken(token);
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var tokenVersion = principal.FindFirst("TokenVersion")?.Value;

                var user = await _userManager.FindByIdAsync(userId);
                if (user != null && user.TokenVersion.ToString() == tokenVersion)
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
