using DomainLayer.Models;
using RepositoryLayer.UnitOfWork_;
using ServiceLayer.DTOs;
using ServiceLayer.Service.Abstraction;
using ServiceLayer.Service.BlockchainService;
using ServiceLayer.Service.Realization.IdentityServices;
using ServiceLayer.Service.Realization.Mapper_;

namespace ServiceLayer.Service.Realization
{
    public class WithdrawalService : IWithdrawalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly MyMapper _mapper;
        private readonly IBitcoinService _bitcoinService;

        public WithdrawalService(IUnitOfWork unitOfWork, IBitcoinService bitcoinService)
        {
            _unitOfWork = unitOfWork;
            _mapper = new MyMapper(unitOfWork);
            _bitcoinService = bitcoinService;
        }

        public async Task<WithdrawalDTO> AddAsync(WithdrawalDTO dto)
        {
            var entity = _mapper.Map<WithdrawalDTO, Withdrawal>(dto);
            var result = await _unitOfWork.Withdrawals.CreateAsync(entity);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<Withdrawal, WithdrawalDTO>(result);
        }

        public async Task<bool> DeleteAsync(WithdrawalDTO dto)
        {
            var entity = _mapper.Map<WithdrawalDTO, Withdrawal>(dto);
            var result = await _unitOfWork.Withdrawals.DeleteAsync(entity);
            await _unitOfWork.CompleteAsync();
            return result;
        }

        public async Task<bool> DeleteById(int id)
        {
            var result = await _unitOfWork.Withdrawals.DeleteByIdAsync(id);
            await _unitOfWork.CompleteAsync();
            return result;
        }

        public async Task<IEnumerable<WithdrawalDTO>> GetAllAsync()
        {
            var entities = await _unitOfWork.Withdrawals.GetAllAsync();
            var dtos = new List<WithdrawalDTO>();
            foreach (var e in entities)
            {
                var tmp = await _mapper.WithdrawalToDTO(e);
                dtos.Add(tmp);
            }
            return dtos;
        }

        public async Task<WithdrawalDTO> GetAsync(int id)
        {
            var entity = await _unitOfWork.Withdrawals.GetByIdAsync(id);
            if (entity == null) throw new ArgumentException("Withdrawal not found");
            return await _mapper.WithdrawalToDTO(entity);
        }

        public async Task<WithdrawalDTO> UpdateAsync(WithdrawalDTO dto)
        {
            var entity = _mapper.Map<WithdrawalDTO, Withdrawal>(dto);
            var result = await _unitOfWork.Withdrawals.UpdateAsync(entity);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<Withdrawal, WithdrawalDTO>(result);
        }

        public async Task<List<WithdrawalDTO>> GetWithdrawalsForReportAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var result = (await GetAllAsync())
                .Where(w => w.UserWallet.UserId == userId && w.RequestedDate >= startDate && w.RequestedDate <= endDate);
            return result.ToList();
        }
    }
}
