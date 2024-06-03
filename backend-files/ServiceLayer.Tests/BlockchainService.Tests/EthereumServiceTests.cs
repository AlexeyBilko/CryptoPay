using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using NUnit.Framework;
using ServiceLayer.DTOs;
using ServiceLayer.Service.BlockchainService;
using Microsoft.Extensions.Configuration;
using Nethereum.Web3;
using Nethereum.Hex.HexTypes;

namespace ServiceLayer.Tests
{
    [TestFixture]
    public class EthereumServiceTests
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<ICryptographyService> _cryptographyServiceMock;
        private EthereumService _ethereumService;

        private const string ApiKey = "test-api-key";

        [SetUp]
        public void SetUp()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _configurationMock = new Mock<IConfiguration>();
            _cryptographyServiceMock = new Mock<ICryptographyService>();

            _configurationMock.Setup(c => c["Blockchain:EthereumApiKey"]).Returns(ApiKey);
            _configurationMock.Setup(c => c["Blockchain:InfuraProjectId"]).Returns("test-infura-project-id");
            _configurationMock.Setup(c => c["Blockchain:UseTestnet"]).Returns("true");

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _ethereumService = new EthereumService(httpClient, _configurationMock.Object, _cryptographyServiceMock.Object);
        }

        [Test]
        public async Task ValidateAddress_ShouldReturnTrue_WhenAddressIsValid()
        {
            // Arrange
            var address = "0xde0B295669a9FD93d5F28D9Ec85E40f4cb697BAe";
            var isTestnet = true;
            var responseContent = JsonConvert.SerializeObject(new { status = "1" });

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _ethereumService.ValidateAddress(address, isTestnet);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task ValidateAddress_ShouldReturnFalse_WhenAddressIsInvalid()
        {
            // Arrange
            var address = "0xde0B295669a9FD93d5F28D9Ec85E40f4cb697BAe";
            var isTestnet = true;
            var responseContent = JsonConvert.SerializeObject(new { status = "0" });

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _ethereumService.ValidateAddress(address, isTestnet);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task VerifyTransactionByHash_ShouldReturnConfirmed()
        {
            // Arrange
            var transactionHash = "0xmockedtransactionhash";
            var isTestnet = true;
            var responseContent = JsonConvert.SerializeObject(new { status = "1", result = new { status = "1" } });

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains(transactionHash)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _ethereumService.VerifyTransactionByHash(transactionHash, isTestnet);

            // Assert
            result.Should().Be("Confirmed");
        }

        [Test]
        public async Task VerifyTransactionByHash_ShouldReturnPending()
        {
            // Arrange
            var transactionHash = "0xmockedtransactionhash";
            var isTestnet = true;
            var responseContent = JsonConvert.SerializeObject(new { status = "1", result = new { status = "0" } });

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains(transactionHash)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _ethereumService.VerifyTransactionByHash(transactionHash, isTestnet);

            // Assert
            result.Should().Be("Pending");
        }

        [Test]
        public async Task VerifyTransactionByHash_ShouldReturnFailed()
        {
            // Arrange
            var transactionHash = "0xmockedtransactionhash";
            var isTestnet = true;
            var responseContent = JsonConvert.SerializeObject(new { status = "0" });

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains(transactionHash)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _ethereumService.VerifyTransactionByHash(transactionHash, isTestnet);

            // Assert
            result.Should().Be("Failed");
        }

        [Test]
        public async Task GetCurrentPriceAsync_ShouldReturnPrice()
        {
            // Arrange
            var price = 2500m;
            var responseContent = JsonConvert.SerializeObject(new { ethereum = new { usd = price } });

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(responseContent)
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("coingecko")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _ethereumService.GetCurrentPriceAsync();

            // Assert
            result.Should().Be(price);
        }
    }
}