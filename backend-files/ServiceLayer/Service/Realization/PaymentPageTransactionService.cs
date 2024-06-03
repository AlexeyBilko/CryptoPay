using DomainLayer.Models;
using RepositoryLayer.UnitOfWork_;
using ServiceLayer.DTOs;
using ServiceLayer.Service.Abstraction;
using ServiceLayer.Service.Realization.IdentityServices;
using ServiceLayer.Service.Realization.Mapper_;

namespace ServiceLayer.Service.Realization
{
    public class PaymentPageTransactionService : IPaymentPageTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly MyMapper _mapper;
        private readonly ICurrencyService _currencyService;

        public PaymentPageTransactionService(IUnitOfWork unitOfWork, ICurrencyService currencyService)
        {
            _unitOfWork = unitOfWork;
            _mapper = new MyMapper(unitOfWork);
            _currencyService = currencyService;
        }

        public async Task<PaymentPageTransactionDTO> AddAsync(PaymentPageTransactionDTO dto)
        {
            var entity = _mapper.FromDTOtoPaymentPageTransaction(dto);
            var result = await _unitOfWork.PaymentPageTransactions.CreateAsync(entity);
            await _unitOfWork.CompleteAsync();
            return await _mapper.PaymentPageTransactionToDTO(result);
        }

        public async Task<bool> DeleteAsync(PaymentPageTransactionDTO dto)
        {
            var entity = _mapper.Map<PaymentPageTransactionDTO, PaymentPageTransaction>(dto);
            var result = await _unitOfWork.PaymentPageTransactions.DeleteAsync(entity);
            await _unitOfWork.CompleteAsync();
            return result;
        }

        public async Task<bool> DeleteById(int id)
        {
            var result = await _unitOfWork.PaymentPageTransactions.DeleteByIdAsync(id);
            await _unitOfWork.CompleteAsync();
            return result;
        }

        public async Task<IEnumerable<PaymentPageTransactionDTO>> GetAllAsync()
        {
            var entities = await _unitOfWork.PaymentPageTransactions.GetAllAsync();
            var dtos = new List<PaymentPageTransactionDTO>();
            foreach (var e in entities)
            {
                var tmp = await _mapper.PaymentPageTransactionToDTO(e);
                dtos.Add(tmp);
            }
            return dtos;
        }

        public async Task<IEnumerable<PaymentPageTransactionDTO>> GetAllByUserAsync(string userId)
        {
            var entities = await _unitOfWork.PaymentPageTransactions.GetAllAsync();
            var dtos = new List<PaymentPageTransactionDTO>();
            foreach (var e in entities)
            {
                var tmp = await _mapper.PaymentPageTransactionToDTO(e);
                if(tmp.PaymentPage.UserId == userId)
                {
                    dtos.Add(tmp);
                }
            }
            return dtos;
        }

        public async Task<PaymentPageTransactionDTO> GetAsync(int id)
        {

            var entity = await _unitOfWork.PaymentPageTransactions.GetByIdAsync(id);
            if (entity == null) throw new ArgumentException("Payment Page Transaction not found");
            return await _mapper.PaymentPageTransactionToDTO(entity);
        }

        public async Task<PaymentPageTransactionDTO> UpdateAsync(PaymentPageTransactionDTO dto)
        {
            var entity = _mapper.Map<PaymentPageTransactionDTO, PaymentPageTransaction>(dto);
            var result = await _unitOfWork.PaymentPageTransactions.UpdateAsync(entity);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<PaymentPageTransaction, PaymentPageTransactionDTO>(result);
        }

        public async Task<PaymentPageTransactionDTO> CompleteTransaction(PaymentPageTransactionDTO dto)
        {
            try
            {
                var transaction = _mapper.Map<PaymentPageTransactionDTO, PaymentPageTransaction>(dto);
                var result = await _unitOfWork.PaymentPageTransactions.UpdateAsync(transaction);
                if (result != null) 
                {
                    await UpdateEarningsAfterTransaction(transaction.PaymentPage.UserId, transaction.PaymentPage.AmountDetails.Currency.CurrencyCode, transaction.ActualAmountCrypto);
                }
                await _unitOfWork.CompleteAsync();
                return _mapper.Map<PaymentPageTransaction, PaymentPageTransactionDTO>(result);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed.");
            }
        }


        public async Task<bool> UpdateEarningsAfterTransaction(string userId, string currencyCode, decimal EarnedCrypto)
        {
            try
            {
                var earnings = (await _unitOfWork.Earnings.GetAllAsync()).Where(e => e.UserId == userId).First();
                if (earnings == null)
                {
                    throw new ArgumentException("Earnings record not found");
                }


                decimal currentPrice = await _currencyService.GetCurrentPriceAsync(currencyCode);

                earnings.TotalEarnedUSD += currentPrice;
                earnings.CurrentBalanceUSD += currentPrice;

                if(currencyCode == "btc")
                {
                    earnings.TotalEarnedBTC += EarnedCrypto;
                    earnings.CurrentBalanceBTC += EarnedCrypto;
                }
                else
                {
                    earnings.TotalEarnedETH += EarnedCrypto;
                    earnings.CurrentBalanceETH += EarnedCrypto;

                }

                await _unitOfWork.Earnings.UpdateAsync(earnings);
                await _unitOfWork.CompleteAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed.");
            }
        }

        public async Task<List<PaymentPageTransactionDTO>> GetTransactionsForReportAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var result = (await GetAllAsync()).Where(w => w.PaymentPage.UserId == userId && w.CreatedAt >= startDate && w.CreatedAt <= endDate);
            return result.ToList();
        }

    }
}
