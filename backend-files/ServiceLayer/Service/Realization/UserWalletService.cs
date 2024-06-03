using DomainLayer.Models;
using RepositoryLayer.UnitOfWork_;
using ServiceLayer.DTOs;
using ServiceLayer.Service.Abstraction;
using ServiceLayer.Service.Realization.IdentityServices;
using ServiceLayer.Service.Realization.Mapper_;

namespace ServiceLayer.Service.Realization
{
    public class UserWalletService : IUserWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly MyMapper _mapper;

        public UserWalletService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _mapper = new MyMapper(unitOfWork);
        }

        public async Task<UserWalletDTO> AddAsync(UserWalletDTO dto)
        {
            var entity = _mapper.Map<UserWalletDTO, UserWallet>(dto);
            var result = await _unitOfWork.UserWallets.CreateAsync(entity);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<UserWallet, UserWalletDTO>(result);
        }

        public async Task<bool> DeleteAsync(UserWalletDTO dto)
        {
            var entity = _mapper.Map<UserWalletDTO, UserWallet>(dto);
            var result = await _unitOfWork.UserWallets.DeleteAsync(entity);
            await _unitOfWork.CompleteAsync();
            return result;
        }

        public async Task<bool> DeleteById(int id)
        {
            var result = await _unitOfWork.UserWallets.DeleteByIdAsync(id);
            await _unitOfWork.CompleteAsync();
            return result;
        }

        public async Task<IEnumerable<UserWalletDTO>> GetAllAsync()
        {
            var entities = await _unitOfWork.UserWallets.GetAllAsync();
            return entities.Select(e => _mapper.Map<UserWallet, UserWalletDTO>(e));
        }

        public async Task<UserWalletDTO> GetAsync(int id)
        {
            var entity = await _unitOfWork.UserWallets.GetByIdAsync(id);
            if (entity == null) throw new ArgumentException("User Wallet not found");
            return _mapper.Map<UserWallet, UserWalletDTO>(entity);
        }

        public async Task<UserWalletDTO> GetByWalletAddress(string walletNumber)
        {
            var entity = (await _unitOfWork.UserWallets.GetAllAsync()).Where(u => u.WalletNumber == walletNumber).FirstOrDefault();
            if (entity == null) return null;
            return _mapper.Map<UserWallet, UserWalletDTO>(entity);
        }

        public async Task<UserWalletDTO> UpdateAsync(UserWalletDTO dto)
        {
            var entity = _mapper.Map<UserWalletDTO, UserWallet>(dto);
            var result = await _unitOfWork.UserWallets.UpdateAsync(entity);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<UserWallet, UserWalletDTO>(result);
        }
    }
}
