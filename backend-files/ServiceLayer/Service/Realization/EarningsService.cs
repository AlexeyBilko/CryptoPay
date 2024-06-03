using DomainLayer.Models;
using RepositoryLayer.UnitOfWork_;
using ServiceLayer.DTOs;
using ServiceLayer.Service.Abstraction;
using ServiceLayer.Service.Realization.IdentityServices;
using ServiceLayer.Service.Realization.Mapper_;
using System.Reflection.Metadata;
using System.Text;

namespace ServiceLayer.Service.Realization
{
    public class EarningsService : IEarningsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly MyMapper _mapper;
        private readonly IPaymentPageTransactionService _paymentPageTransactionService;
        private readonly ICurrencyService _currencyService;

        public EarningsService(IUnitOfWork unitOfWork, IPaymentPageTransactionService paymentPageTransactionService, ICurrencyService currencyService)
        {
            _unitOfWork = unitOfWork;
            _mapper = new MyMapper(unitOfWork);
            _paymentPageTransactionService = paymentPageTransactionService;
            _currencyService = currencyService;
        }

        public async Task<EarningsDTO> AddAsync(EarningsDTO dto)
        {
            var entity = _mapper.Map<EarningsDTO, Earnings>(dto);
            var result = await _unitOfWork.Earnings.CreateAsync(entity);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<Earnings, EarningsDTO>(result);
        }

        public async Task<bool> DeleteAsync(EarningsDTO dto)
        {
            var entity = _mapper.Map<EarningsDTO, Earnings>(dto);
            var result = await _unitOfWork.Earnings.DeleteAsync(entity);
            await _unitOfWork.CompleteAsync();
            return result;
        }

        public async Task<bool> DeleteById(int id)
        {
            var result = await _unitOfWork.Earnings.DeleteByIdAsync(id);
            await _unitOfWork.CompleteAsync();
            return result;
        }

        public async Task<IEnumerable<EarningsDTO>> GetAllAsync()
        {
            var entities = await _unitOfWork.Earnings.GetAllAsync();
            return entities.Select(e => _mapper.Map<Earnings, EarningsDTO>(e));
        }

        public async Task<EarningsDTO> GetAsync(int id)
        {
            var entity = await _unitOfWork.Earnings.GetByIdAsync(id);
            if (entity == null) throw new ArgumentException("Earnings not found");
            return _mapper.Map<Earnings, EarningsDTO>(entity);
        }

        public async Task<EarningsDTO> UpdateAsync(EarningsDTO dto)
        {
            var entity = _mapper.Map<EarningsDTO, Earnings>(dto);
            var result = await _unitOfWork.Earnings.UpdateAsync(entity);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<Earnings, EarningsDTO>(result);
        }

        public async Task<bool> UpdateEarningsUSDForUser(string userId)
        {
            var btcToUsdRate = await _currencyService.GetCurrentPriceAsync("BTC");
            var ethToUsdRate = await _currencyService.GetCurrentPriceAsync("ETH");

            var userEarnings = (await GetAllAsync()).Where(e => e.UserId == userId).First();
            userEarnings.CurrentBalanceUSD = (userEarnings.CurrentBalanceBTC * btcToUsdRate) + (userEarnings.CurrentBalanceETH * ethToUsdRate);
            userEarnings.TotalEarnedUSD = (userEarnings.TotalEarnedBTC * btcToUsdRate) + (userEarnings.TotalEarnedETH * ethToUsdRate);

            await UpdateAsync(userEarnings);

            return true;
        }

        public async Task<EarningsDTO> GetEarningsForReportAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var transactions = (await _paymentPageTransactionService
                .GetAllAsync()).Where(t => t.PaymentPage.UserId == userId && t.Status == "Successful" && t.CreatedAt >= startDate && t.CreatedAt <= endDate)
                .ToList();

            var btcEarnings = transactions.Where(t => t.PaymentPage.AmountDetails.Currency.CurrencyCode.ToLower() == "btc").Sum(t => t.ActualAmountCrypto);
            var ethEarnings = transactions.Where(t => t.PaymentPage.AmountDetails.Currency.CurrencyCode.ToLower() == "eth").Sum(t => t.ActualAmountCrypto);

            var btcToUsdRate = await _currencyService.GetCurrentPriceAsync("BTC");
            var ethToUsdRate = await _currencyService.GetCurrentPriceAsync("ETH");

            var totalEarningsUSD = (btcEarnings * btcToUsdRate) + (ethEarnings * ethToUsdRate);

            var userEarnings = (await GetAllAsync()).Where(e => e.UserId == userId).First();
            await UpdateEarningsUSDForUser(userId);

            return new EarningsDTO
            {
                TotalEarnedBTC = btcEarnings,
                TotalEarnedETH = ethEarnings,
                TotalEarnedUSD = totalEarningsUSD,
                CurrentBalanceBTC = userEarnings.CurrentBalanceBTC,
                CurrentBalanceETH = userEarnings.CurrentBalanceETH,
                CurrentBalanceUSD = userEarnings.CurrentBalanceUSD,
            };
        }

        public async Task<EarningsDTO> GetEarningsByUserId(string userId)
        {
            var earnings = await _unitOfWork.Earnings.GetAllAsync();
            return earnings.Where(e => e.UserId == userId)
                .Select(e => _mapper.Map<Earnings, EarningsDTO>(e))
                .FirstOrDefault();
        }

    }
}
