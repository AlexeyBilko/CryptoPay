using DomainLayer.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RepositoryLayer.UnitOfWork_;
using ServiceLayer.DTOs;
using ServiceLayer.Service.Abstraction;
using ServiceLayer.Service.BlockchainService;
using ServiceLayer.Service.Realization.IdentityServices;
using ServiceLayer.Service.Realization.Mapper_;

namespace ServiceLayer.Service.Realization
{
    public class CurrencyService : ICurrencyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly MyMapper _mapper;
        private readonly HttpClient _httpClient;

        private readonly IBitcoinService _bitcoinService;
        private readonly IEthereumService _ethereumService;

        public CurrencyService(IUnitOfWork unitOfWork, HttpClient httpClient, IBitcoinService bitcoinService, IEthereumService ethereumService)
        {
            _unitOfWork = unitOfWork;
            _mapper = new MyMapper(unitOfWork);
            _httpClient = httpClient;


            _bitcoinService = bitcoinService;
            _ethereumService = ethereumService;
        }

        public async Task<CurrencyDTO> AddAsync(CurrencyDTO dto)
        {
            var entity = _mapper.Map<CurrencyDTO, Currency>(dto);
            var result = await _unitOfWork.Currencies.CreateAsync(entity);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<Currency, CurrencyDTO>(result);
        }

        public async Task<bool> DeleteAsync(CurrencyDTO dto)
        {
            var entity = _mapper.Map<CurrencyDTO, Currency>(dto);
            var result = await _unitOfWork.Currencies.DeleteAsync(entity);
            await _unitOfWork.CompleteAsync();
            return result;
        }

        public async Task<bool> DeleteById(int id)
        {
            var result = await _unitOfWork.Currencies.DeleteByIdAsync(id);
            await _unitOfWork.CompleteAsync();
            return result;
        }

        public async Task<IEnumerable<CurrencyDTO>> GetAllAsync()
        {
            var entities = await _unitOfWork.Currencies.GetAllAsync();
            return entities.Select(e => _mapper.Map<Currency, CurrencyDTO>(e));
        }

        public async Task<CurrencyDTO> GetAsync(int id)
        {
            var entity = await _unitOfWork.Currencies.GetByIdAsync(id);
            if (entity == null) throw new ArgumentException("Currency not found");
            return _mapper.Map<Currency, CurrencyDTO>(entity);
        }

        public async Task<CurrencyDTO> GetByCurrencyCode(string currencyCode)
        {
            var entity = (await _unitOfWork.Currencies.GetAllAsync()).Where(c => c.CurrencyCode == currencyCode).First();
            if (entity == null) throw new ArgumentException("Currency not found");
            return _mapper.Map<Currency, CurrencyDTO>(entity);
        }

        public async Task<CurrencyDTO> UpdateAsync(CurrencyDTO dto)
        {
            var entity = _mapper.Map<CurrencyDTO, Currency>(dto);
            var result = await _unitOfWork.Currencies.UpdateAsync(entity);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<Currency, CurrencyDTO>(result);
        }

        public async Task<decimal> GetCurrentPriceAsync(string currencyCode)
        {
            try
            {
                if (currencyCode.ToLower() == "btc") currencyCode = "bitcoin";
                else if (currencyCode.ToLower() == "eth") currencyCode = "ethereum";
                string uri = $"https://api.coingecko.com/api/v3/simple/price?ids={currencyCode.ToLower()}&vs_currencies=usd";
                HttpResponseMessage response = await _httpClient.GetAsync(uri);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, decimal>>>(json);
                    return data[currencyCode.ToLower()]["usd"];
                }
                else
                {
                    throw new Exception("Failed to retrieve currency data.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching currency prices.", ex);
            }
        }

        public async Task<decimal> ConvertCryptoToUSD(decimal cryptoAmount, string currencyCode)
        {
            try
            {
                decimal usdPrice = currencyCode.ToLower() switch
                {
                    "btc" => await _bitcoinService.GetBitcoinPriceInUSD(),
                    "eth" => await _ethereumService.GetEthereumPriceInUSD(),
                    _ => throw new Exception("Invalid currency code")
                };
                return cryptoAmount * usdPrice;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting crypto to USD: {ex.Message}");
            }
        }

        public async Task<decimal> ConvertUSDToCrypto(decimal usdAmount, string currencyCode)
        {
            try
            {
                decimal usdPrice = currencyCode.ToLower() switch
                {
                    "btc" => await _bitcoinService.GetBitcoinPriceInUSD(),
                    "eth" => await _ethereumService.GetEthereumPriceInUSD(),
                    _ => throw new Exception("Invalid currency code")
                };
                return usdAmount / usdPrice;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting USD to crypto: {ex.Message}");
            }
        }
    }
}
