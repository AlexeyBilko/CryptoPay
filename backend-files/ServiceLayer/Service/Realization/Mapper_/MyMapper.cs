using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DomainLayer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using RepositoryLayer.UnitOfWork_;
using ServiceLayer.DTOs;
using ServiceLayer.Service.Realization.IdentityServices;

namespace ServiceLayer.Service.Realization.Mapper_
{
    public class MyMapper
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public MyMapper(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<User, UserDTO>().ReverseMap();
                cfg.CreateMap<Currency, CurrencyDTO>().ReverseMap();
                cfg.CreateMap<SystemWallet, SystemWalletDTO>().ReverseMap();
                cfg.CreateMap<AmountDetails, AmountDetailsDTO>().ReverseMap();
                cfg.CreateMap<Earnings, EarningsDTO>().ReverseMap();
                cfg.CreateMap<PaymentPage, PaymentPageDTO>().ReverseMap();
                cfg.CreateMap<PaymentPageTransaction, PaymentPageTransactionDTO>().ReverseMap();
                cfg.CreateMap<UserWallet, UserWalletDTO>().ReverseMap();
                cfg.CreateMap<Withdrawal, WithdrawalDTO>().ReverseMap();
            });

            _mapper = config.CreateMapper();
        }

        public TDestination Map<TSource, TDestination>(TSource source)
        {
            return _mapper.Map<TSource, TDestination>(source);
        }

        public async Task<TDestination> MapAsync<TSource, TDestination>(TSource source)
        {
            return await Task.Run(() => _mapper.Map<TSource, TDestination>(source));
        }

        public async Task<PaymentPageDTO> PaymentPageToDTO(PaymentPage toConvert)
        {
            var amountDetails = await _unitOfWork.AmountDetails.GetByIdAsync(toConvert.AmountDetailsId);
            var systemWallet = await _unitOfWork.SystemWallets.GetByIdAsync(toConvert.SystemWalletId);

            return new PaymentPageDTO()
            {
                Id = toConvert.Id,
                Title = toConvert.Title,
                Description = toConvert.Description,
                IsDonation = toConvert.IsDonation,
                UserId = toConvert.UserId,
                AmountDetailsId = toConvert.AmountDetailsId,
                AmountDetails = await AmountDetailsToDTO(amountDetails),
                SystemWalletId = toConvert.SystemWalletId,
                SystemWallet = _mapper.Map<SystemWallet, SystemWalletDTO>(systemWallet)
            };
        }

        public PaymentPage FromDTOtoPaymentPage(PaymentPageDTO toConvert)
        {
            return new PaymentPage()
            {
                Id = toConvert.Id,
                Title = toConvert.Title,
                Description = toConvert.Description,
                IsDonation = toConvert.IsDonation,
                UserId = toConvert.UserId,
                AmountDetailsId = toConvert.AmountDetailsId,
                SystemWalletId = toConvert.SystemWalletId
            };
        }

        public async Task<PaymentPageTransactionDTO> PaymentPageTransactionToDTO(PaymentPageTransaction toConvert)
        {
            var paymentPage = await _unitOfWork.PaymentPages.GetByIdAsync(toConvert.PaymentPageId);

            return new PaymentPageTransactionDTO()
            {
                Id = toConvert.Id,
                TransactionHash = toConvert.TransactionHash,
                SenderWalletAddress = toConvert.SenderWalletAddress,
                Status = toConvert.Status,
                CreatedAt = toConvert.CreatedAt,
                BlockNumber = toConvert.BlockNumber,
                BlockTimestamp = toConvert.BlockTimestamp,
                TransactionIndex = toConvert.TransactionIndex,
                GasPrice = toConvert.GasPrice,
                GasUsed = toConvert.GasUsed,
                InputData = toConvert.InputData,
                TransactionFee = toConvert.TransactionFee,
                ActualAmountCrypto = toConvert.ActualAmountCrypto,
                PayerEmail = toConvert.PayerEmail,
                PaymentPageId = toConvert.PaymentPageId,
                PaymentPage = await PaymentPageToDTO(paymentPage)
            };
        }

        public PaymentPageTransaction FromDTOtoPaymentPageTransaction(PaymentPageTransactionDTO toConvert)
        {
            return new PaymentPageTransaction()
            {
                Id = toConvert.Id,
                TransactionHash = toConvert.TransactionHash,
                SenderWalletAddress = toConvert.SenderWalletAddress,
                Status = toConvert.Status,
                CreatedAt = toConvert.CreatedAt,
                BlockNumber = toConvert.BlockNumber,
                BlockTimestamp = toConvert.BlockTimestamp,
                TransactionIndex = toConvert.TransactionIndex,
                GasPrice = toConvert.GasPrice,
                GasUsed = toConvert.GasUsed,
                InputData = toConvert.InputData,
                TransactionFee = toConvert.TransactionFee,
                ActualAmountCrypto = toConvert.ActualAmountCrypto,
                PayerEmail = toConvert.PayerEmail,
                PaymentPageId = toConvert.PaymentPageId
            };
        }

        public async Task<WithdrawalDTO> WithdrawalToDTO(Withdrawal toConvert)
        {
            var systemWallet = await _unitOfWork.SystemWallets.GetByIdAsync(toConvert.SystemWalletId);
            var userWallet = await _unitOfWork.UserWallets.GetByIdAsync(toConvert.UserWalletId);
            var amountDetails = await _unitOfWork.AmountDetails.GetByIdAsync(toConvert.AmountDetailsId);

            return new WithdrawalDTO()
            {
                Id = toConvert.Id,
                SystemWalletId = toConvert.SystemWalletId,
                SystemWallet = _mapper.Map<SystemWallet, SystemWalletDTO>(systemWallet),
                UserWalletId = toConvert.UserWalletId,
                UserWallet = await UserWalletToDTO(userWallet),
                AmountDetailsId = toConvert.AmountDetailsId,
                AmountDetails = await AmountDetailsToDTO(amountDetails),
                Status = toConvert.Status,
                TransactionId = toConvert.TransactionId,
                RequestedDate = toConvert.RequestedDate,
                CompletedDate = toConvert.CompletedDate
            };
        }

        public Withdrawal FromDTOtoWithdrawal(WithdrawalDTO toConvert)
        {
            return new Withdrawal()
            {
                Id = toConvert.Id,
                SystemWalletId = toConvert.SystemWalletId,
                UserWalletId = toConvert.UserWalletId,
                AmountDetailsId = toConvert.AmountDetailsId,
                Status = toConvert.Status,
                TransactionId = toConvert.TransactionId,
                RequestedDate = toConvert.RequestedDate,
                CompletedDate = toConvert.CompletedDate
            };
        }

        public async Task<UserWalletDTO> UserWalletToDTO(UserWallet toConvert)
        {
            return new UserWalletDTO()
            {
                Id = toConvert.Id,
                WalletNumber = toConvert.WalletNumber,
                UserId = toConvert.UserId
            };
        }

        public UserWallet FromDTOtoUserWallet(UserWalletDTO toConvert)
        {
            return new UserWallet()
            {
                Id = toConvert.Id,
                WalletNumber = toConvert.WalletNumber,
                UserId = toConvert.UserId
            };
        }

        public async Task<AmountDetailsDTO> AmountDetailsToDTO(AmountDetails toConvert)
        {
            var currency = await _unitOfWork.Currencies.GetByIdAsync(toConvert.CurrencyId);

            return new AmountDetailsDTO()
            {
                Id = toConvert.Id,
                AmountUSD = toConvert.AmountUSD,
                AmountCrypto = toConvert.AmountCrypto,
                Currency = _mapper.Map<Currency, CurrencyDTO>(currency)
            };
        }

        public AmountDetails FromDTOtoAmountDetails(AmountDetailsDTO toConvert)
        {
            return new AmountDetails()
            {
                Id = toConvert.Id,
                AmountUSD = toConvert.AmountUSD,
                AmountCrypto = toConvert.AmountCrypto,
                CurrencyId = toConvert.CurrencyId
            };
        }
    }

    public static class WhenAllHelper
    {
        public static async Task<IEnumerable<T>> WhenAll<T>(this IEnumerable<Task<T>> tasks)
        {
            return await Task.WhenAll(tasks);
        }
    }
}
