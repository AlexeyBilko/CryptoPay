using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using ServiceLayer.DTOs;
using ServiceLayer.Service.Realization.IdentityServices;
using DomainLayer.Models;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace ServiceLayer.Tests
{
    [TestFixture]
    public class JwtTokenServiceTests
    {
        private Mock<UserManager<User>> _userManagerMock;
        private Mock<IConfiguration> _configurationMock;
        private JwtTokenService _jwtTokenService;

        [SetUp]
        public void SetUp()
        {
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            _configurationMock = new Mock<IConfiguration>();
            _configurationMock.Setup(config => config["JwtSettings:SecretKey"]).Returns("testkey1234567890");
            _configurationMock.Setup(config => config["JwtSettings:Issuer"]).Returns("testIssuer");
            _configurationMock.Setup(config => config["JwtSettings:Audience"]).Returns("testAudience");

            _jwtTokenService = new JwtTokenService(_configurationMock.Object, _userManagerMock.Object);
        }

        [Test]
        public void GenerateToken_ShouldReturnValidToken()
        {
            // Arrange
            var user = new User { Id = "testuser", Email = "test@example.com", TokenVersion = 1 };

            // Act
            var result = _jwtTokenService.GenerateToken(user);

            // Assert
            result.Should().NotBeNullOrEmpty();
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("testkey1234567890");
            tokenHandler.ValidateToken(result, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = "testIssuer",
                ValidateAudience = true,
                ValidAudience = "testAudience",
                ValidateLifetime = true
            }, out SecurityToken validatedToken);

            validatedToken.Should().NotBeNull();
        }

        [Test]
        public async Task GenerateTokensAsync_ShouldReturnValidTokens()
        {
            // Arrange
            var user = new User { Id = "testuser", Email = "test@example.com", TokenVersion = 1 };

            // Act
            var result = await _jwtTokenService.GenerateTokensAsync(user);

            // Assert
            result.Should().NotBeNull();
            result.JwtToken.Should().NotBeNullOrEmpty();
            result.RefreshToken.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void GetPrincipalFromExpiredToken_ShouldReturnValidPrincipal()
        {
            // Arrange
            var user = new User { Id = "testuser", Email = "test@example.com", TokenVersion = 1 };
            var token = _jwtTokenService.GenerateToken(user);

            // Act
            var principal = _jwtTokenService.GetPrincipalFromExpiredToken(token);

            // Assert
            principal.Should().NotBeNull();
            principal.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == user.Id);
        }

        [Test]
        public async Task IsTokenValid_ShouldReturnTrueForValidToken()
        {
            // Arrange
            var user = new User { Id = "testuser", Email = "test@example.com", TokenVersion = 1 };
            var token = _jwtTokenService.GenerateToken(user);

            _userManagerMock.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);

            // Act
            var result = await _jwtTokenService.IsTokenValid(token);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task IsTokenValid_ShouldReturnFalseForInvalidToken()
        {
            // Arrange
            var user = new User { Id = "testuser", Email = "test@example.com", TokenVersion = 1 };
            var invalidToken = "invalidToken";

            // Act
            var result = await _jwtTokenService.IsTokenValid(invalidToken);

            // Assert
            result.Should().BeFalse();
        }
    }
}
