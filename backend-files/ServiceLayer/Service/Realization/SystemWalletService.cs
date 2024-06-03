using DomainLayer.Models;
using RepositoryLayer.UnitOfWork_;
using ServiceLayer.DTOs;
using ServiceLayer.Service.Abstraction;
using ServiceLayer.Service.BlockchainService;
using ServiceLayer.Service.Realization.IdentityServices;
using ServiceLayer.Service.Realization.Mapper_;

namespace ServiceLayer.Service.Realization
{
    public class SystemWalletService : ISystemWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly MyMapper _mapper;
        private readonly ICryptographyService _cryptographyService;

        private readonly IBitcoinService _bitcoinService;
        private readonly IEthereumService _ethereumService;


        public SystemWalletService(IUnitOfWork unitOfWork, ICryptographyService cryptographyService, IBitcoinService bitcoinService, IEthereumService ethereumService)
        {
            _unitOfWork = unitOfWork;
            _mapper = new MyMapper(unitOfWork);
            _cryptographyService = cryptographyService;
            _bitcoinService = bitcoinService;
            _ethereumService = ethereumService;
        }

        public async Task<SystemWalletDTO> AddAsync(SystemWalletDTO dto)
        {
            dto.EncryptedWalletCodePhrase = _cryptographyService.Encrypt(dto.EncryptedWalletCodePhrase);
            var entity = _mapper.Map<SystemWalletDTO, SystemWallet>(dto);
            var result = await _unitOfWork.SystemWallets.CreateAsync(entity);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<SystemWallet, SystemWalletDTO>(result);
        }

        public async Task<bool> DeleteAsync(SystemWalletDTO dto)
        {
            var entity = _mapper.Map<SystemWalletDTO, SystemWallet>(dto);
            var result = await _unitOfWork.SystemWallets.DeleteAsync(entity);
            await _unitOfWork.CompleteAsync();
            return result;
        }

        public async Task<bool> DeleteById(int id)
        {
            var result = await _unitOfWork.SystemWallets.DeleteByIdAsync(id);
            await _unitOfWork.CompleteAsync();
            return result;
        }

        public async Task<IEnumerable<SystemWalletDTO>> GetAllAsync()
        {
            var entities = await _unitOfWork.SystemWallets.GetAllAsync();
            return entities.Select(e => _mapper.Map<SystemWallet, SystemWalletDTO>(e));
        }

        public async Task<SystemWalletDTO> GetAsync(int id)
        {
            var entity = await _unitOfWork.SystemWallets.GetByIdAsync(id);
            if (entity == null) throw new ArgumentException("System Wallet not found");
            return _mapper.Map<SystemWallet, SystemWalletDTO>(entity);
        }

        public async Task<SystemWalletDTO> UpdateAsync(SystemWalletDTO dto)
        {
            var entity = _mapper.Map<SystemWalletDTO, SystemWallet>(dto);
            var result = await _unitOfWork.SystemWallets.UpdateAsync(entity);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<SystemWallet, SystemWalletDTO>(result);
        }

        public async Task<SystemWalletDTO> GetSuitableWalletForCurrency(string currencyCode)
        {
            var wallets = await _unitOfWork.SystemWallets.GetAllAsync();

            foreach (var wallet in wallets)
            {
                if (currencyCode.ToLower() == "btc" && (await _bitcoinService.ValidateAddress(wallet.WalletNumber, true)))
                {
                    return _mapper.Map<SystemWallet, SystemWalletDTO>(wallet);
                }
                else if (currencyCode.ToLower() == "eth" && (await _ethereumService.ValidateAddress(wallet.WalletNumber, true)))
                {
                    return _mapper.Map<SystemWallet, SystemWalletDTO>(wallet);
                }
            }

            throw new InvalidOperationException($"No suitable wallet found for currency code: {currencyCode}");
        }

        //public async Task<SystemWalletDTO> GetSuitableWalletForCurrency(string currencyCode)
        //{
        //    var wallet = (await _unitOfWork.SystemWallets.GetAllAsync())
        //                    .Where(w => w.Currency.CurrencyCode == currencyCode)
        //                    .FirstOrDefault();

        //    if (wallet == null)
        //    {
        //        return null;
        //    }

        //    return _mapper.Map<SystemWallet, SystemWalletDTO>(wallet);
        //}
    }
}
