using System;
using System.Collections.Generic;
using System.Linq;
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
using NBitcoin;
using QBitNinja.Client.Models;
using QBitNinja.Client;

namespace ServiceLayer.Tests
{
    [TestFixture]
    public class BitcoinServiceTests
    {
        private Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private Mock<HttpClient> _httpClientMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<ICryptographyService> _cryptographyServiceMock;
        private IBitcoinService _bitcoinService;

        private const string ApiKey = "test-api-key";

        [SetUp]
        public void SetUp()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClientMock = new Mock<HttpClient>(_httpMessageHandlerMock.Object) { CallBase = true };
            _configurationMock = new Mock<IConfiguration>();
            _cryptographyServiceMock = new Mock<ICryptographyService>();

            _configurationMock.Setup(c => c["Blockchain:BitcoinApiKey"]).Returns(ApiKey);

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _bitcoinService = new BitcoinService(httpClient, _configurationMock.Object, _cryptographyServiceMock.Object);
        }

        [Test]
        public async Task ValidateAddress_ShouldReturnTrue_WhenAddressIsValid()
        {
            // Arrange
            var address = "valid-testnet-address";
            var isTestnet = true;
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _bitcoinService.ValidateAddress(address, isTestnet);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task ValidateAddress_ShouldReturnFalse_WhenAddressIsInvalid()
        {
            // Arrange
            var address = "invalid-testnet-address";
            var isTestnet = true;
            var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _bitcoinService.ValidateAddress(address, isTestnet);

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        public async Task GetTransactionFeeAsync_ShouldReturnFee()
        {
            // Arrange
            var isTestnet = true;
            var feeData = new { fees = new { medium_fee_per_kb = 1000 } };
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(feeData))
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _bitcoinService.GetTransactionFeeAsync(isTestnet);

            // Assert
            result.Should().Be(1000);
        }

        [Test]
        public async Task GetTransactionFeeAsync_ShouldThrowException_WhenResponseIsNotSuccessful()
        {
            // Arrange
            var isTestnet = true;
            var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act & Assert
            Func<Task> act = async () => await _bitcoinService.GetTransactionFeeAsync(isTestnet);
            await act.Should().ThrowAsync<Exception>().WithMessage("Failed to fetch transaction fees.");
        }

        [Test]
        public async Task GetWalletBalanceAsync_ShouldReturnBalance()
        {
            // Arrange
            var walletAddress = "test-wallet-address";
            var isTestnet = true;
            var balanceData = new { final_balance = 100000000 }; // 1 BTC in satoshis
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(balanceData))
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _bitcoinService.GetWalletBalanceAsync(walletAddress, isTestnet);

            // Assert
            result.Should().Be(1);
        }

        [Test]
        public async Task GetWalletBalanceAsync_ShouldThrowException_WhenResponseIsNotSuccessful()
        {
            // Arrange
            var walletAddress = "test-wallet-address";
            var isTestnet = true;
            var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act & Assert
            Func<Task> act = async () => await _bitcoinService.GetWalletBalanceAsync(walletAddress, isTestnet);
            await act.Should().ThrowAsync<Exception>().WithMessage("Failed to fetch wallet balance.");
        }


        [Test]
        public async Task BroadcastTransaction_ShouldReturnTransactionHash()
        {
            // Arrange
            var isTestnet = true;
            var transactionHash = "transaction-hash";
            var tx = Network.TestNet.CreateTransaction();

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new { tx = new { hash = transactionHash } }))
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act
            var result = await _bitcoinService.BroadcastTransaction(tx, isTestnet);

            // Assert
            result.Should().Be(transactionHash);
        }

        [Test]
        public async Task BroadcastTransaction_ShouldThrowException_WhenResponseIsNotSuccessful()
        {
            // Arrange
            var isTestnet = true;
            var tx = Network.TestNet.CreateTransaction();
            var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("error message")
            };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);

            // Act & Assert
            Func<Task> act = async () => await _bitcoinService.BroadcastTransaction(tx, isTestnet);
            await act.Should().ThrowAsync<Exception>().WithMessage("Failed to broadcast transaction: error message");
        }
    }
}
