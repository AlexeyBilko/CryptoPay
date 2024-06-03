using Microsoft.AspNetCore.Mvc;
using ServiceLayer.Service.Realization.IdentityServices;
using ServiceLayer.DTOs;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using System;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using ServiceLayer.Service.Realization;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly JwtTokenService _jwtTokenService;
    private readonly EmailService _emailService;

    public AuthController(UserService userService, JwtTokenService jwtTokenService, EmailService emailService)
    {
        _userService = userService;
        _jwtTokenService = jwtTokenService;
        _emailService = emailService;
    }

    public class EmailVerificationDTO
    {
        public string Email { get; set; }
    }

    [AllowAnonymous]
    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] EmailVerificationDTO model)
    {
        var user = await _userService.FindByEmailAsync(model.Email);
        if (user != null)
        {
            return BadRequest("User already exists.");
        }

        var verificationCode = new Random().Next(100000, 999999).ToString();
        var subject = "Ваш код верифікації для CryptoPay";
        var message = $"Ваш код верифікації: {verificationCode}<br></br>Будь-ласка скопіюйте його і ставте в діалогове вікно на сторінці реєстрації.";

        await _emailService.SendEmailAsync(model.Email, subject, message);

        return Ok(new { VerificationCode = verificationCode });
    }

    public class RegisterRequest
    {
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string DisplayName { get; set; }
    }

    /// <summary>
    /// Registers a new user with a given UserDTO and password.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        try
        {
            UserDTO userDTO = new UserDTO()
            {
                Id = "",
                Email = registerRequest.Email,
                PasswordHash = registerRequest.PasswordHash,
                DisplayName = registerRequest.DisplayName,
                RegistrationDate = DateTime.Now,
                Preferences = "none",
                RefreshToken = "",
                RefreshTokenExpiryTime = DateTime.Now,
                RefreshTokenVersion = 0,
                TokenVersion = 0
            };
            var user = await _userService.RegisterAsync(userDTO);
            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    /// <summary>
    /// Logs in a user with a given email and password, returning a token pair.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        try
        {
            var tokens = await _userService.LoginAsync(loginRequest.Email, loginRequest.Password);
            if (tokens != null)
                return Ok(tokens);
            return Unauthorized("Credentials are not valid.");
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Restores a user's password, accepting an email and a new password.
    /// </summary>
    [HttpPost("restore-password")]
    public async Task<IActionResult> RestorePassword(string email, string newPassword)
    {
        try
        {
            var result = await _userService.RestorePassword(email, newPassword);
            if (result.Succeeded)
                return Ok();
            return BadRequest(new { Errors = result.Errors });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Logs out the user, invalidating their session tokens.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated.");
            }

            var result = await _userService.LogoutAsync(userId);
            if (result)
            {
                return Ok(new { message = "Logged out successfully." });
            }
            return BadRequest("Failed to log out.");
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Changes the password of a user given their current and new password details.
    /// </summary>
    [HttpPut("updatePassword")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordRequest model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userService.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userService.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok();
    }

    public class UpdateDisplayNameRequest
    {
        public string DisplayName { get; set; }
    }

    public class UpdatePasswordRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    [HttpGet("get-user-claims")]
    public IActionResult GetUserClaims()
    {
        var claimsList = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        return Ok(claimsList);
    }

    [AllowAnonymous]
    [HttpPost("verify-token")]
    public async Task<IActionResult> VerifyToken([FromBody] TokenRequest tokenRequest)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("CryptoPaymentGatewaySecretKey");

            tokenHandler.ValidateToken(tokenRequest.Token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = "http://localhost:5001",
                ValidateAudience = true,
                ValidAudience = "http://localhost:5001",
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            return Ok(new { IsValid = true });
        }
        catch (SecurityTokenException)
        {
            return BadRequest(new { IsValid = false, Message = "Invalid token" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { IsValid = false, Message = ex.Message });
        }
    }

    /// <summary>
    /// Renews a session with an expired token and a refresh token.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("renew-token")]
    public async Task<IActionResult> RenewToken([FromBody] TokenRequest tokenRequest)
    {
        try
        {
            var tokens = await _jwtTokenService.RenewTokensAsync(tokenRequest.Token);
            return Ok(tokens);
        }
        catch (SecurityTokenException ex)
        {
            return Unauthorized(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    public class TokenRequest
    {
        public string Token { get; set; }
    }


    /// <summary>
    /// Retrieves the user details of the authenticated user.
    /// </summary>
    [HttpGet("user-details")]
    public async Task<IActionResult> GetUserDetails()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated.");
            }
            var user = await _userService.FindByIdAsync(userId);
            if (user != null)
            {
                return Ok(user);
            }
            
            return NotFound("User not found.");
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    /// <summary>
    /// Updates the user profile details.
    /// </summary>
    [HttpPut("update-profile")]
    public async Task<IActionResult> UpdateUserProfile(UserDTO userDto)
    {
        try
        {
            var result = await _userService.UpdateUserProfileAsync(userDto);
            if (result.Succeeded)
            {
                return Ok(new { Message = "Profile updated successfully." });
            }
            return BadRequest(new { Errors = result.Errors });
        }
        catch (Exception ex)
        {
            return BadRequest(new { Message = ex.Message });
        }
    }

    [HttpPut("updateDisplayName")]
    public async Task<IActionResult> UpdateDisplayName([FromBody] UpdateDisplayNameRequest model)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = await _userService.FindByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        user.DisplayName = model.DisplayName;
        var result = await _userService.UpdateUserProfileAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }

        return Ok();
    }
}
