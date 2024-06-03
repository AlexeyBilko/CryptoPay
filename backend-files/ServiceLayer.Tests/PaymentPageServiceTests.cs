using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using DomainLayer.Models;
using RepositoryLayer.UnitOfWork_;
using ServiceLayer.DTOs;
using ServiceLayer.Service.Abstraction;
using ServiceLayer.Service.Realization;
using ServiceLayer.Service.Realization.IdentityServices;
using ServiceLayer.Service.Realization.Mapper_;

namespace ServiceLayer.Tests
{
    [TestFixture]
    public class PaymentPageServiceTests
    {
        private Mock<IUnitOfWork> _unitOfWorkMock;
        private PaymentPageService _paymentPageService;
        private MyMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _mapper = new MyMapper(_unitOfWorkMock.Object);
            _paymentPageService = new PaymentPageService(_unitOfWorkMock.Object);
        }

        [Test]
        public async Task AddAsync_ShouldReturnAddedPaymentPageDTO()
        {
            // Arrange
            var dto = new PaymentPageDTO { Id = 1, UserId = "user1", Title = "Test Page" };
            var entity = _mapper.FromDTOtoPaymentPage(dto);
            var returnedDto = _mapper.Map<PaymentPage, PaymentPageDTO>(entity);

            _unitOfWorkMock.Setup(u => u.PaymentPages.CreateAsync(It.IsAny<PaymentPage>())).ReturnsAsync(entity);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _paymentPageService.AddAsync(dto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(returnedDto, options => options.ExcludingMissingMembers());
        }

        [Test]
        public async Task DeleteAsync_ShouldReturnTrueIfDeletionSucceeds()
        {
            // Arrange
            var dto = new PaymentPageDTO { Id = 1, UserId = "user1", Title = "Test Page" };
            var entity = _mapper.FromDTOtoPaymentPage(dto);

            _unitOfWorkMock.Setup(u => u.PaymentPages.DeleteAsync(It.IsAny<PaymentPage>())).ReturnsAsync(true);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _paymentPageService.DeleteAsync(dto);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public async Task DeleteById_ShouldReturnTrueIfDeletionSucceeds()
        {
            // Arrange
            int id = 1;

            _unitOfWorkMock.Setup(u => u.PaymentPages.DeleteByIdAsync(id)).ReturnsAsync(true);
            _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

            // Act
            var result = await _paymentPageService.DeleteById(id);

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        public void GetAsync_ShouldThrowArgumentExceptionIfPaymentPageNotFound()
        {
            // Arrange
            int id = 1;

            _unitOfWorkMock.Setup(u => u.PaymentPages.GetByIdAsync(id)).ReturnsAsync((PaymentPage)null);

            // Act & Assert
            Func<Task> act = async () => await _paymentPageService.GetAsync(id);
            act.Should().ThrowAsync<ArgumentException>().WithMessage("Payment Page not found");
        }
    }
}
