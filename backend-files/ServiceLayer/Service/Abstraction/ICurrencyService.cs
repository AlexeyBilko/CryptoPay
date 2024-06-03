using DomainLayer.Models;
using ServiceLayer.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Service.Abstraction
{
    public interface ICurrencyService : IService<Currency, CurrencyDTO, int>
    {
        Task<CurrencyDTO> GetByCurrencyCode(string currencyCode);
        Task<decimal> GetCurrentPriceAsync(string currencyCode);
        Task<decimal> ConvertCryptoToUSD(decimal cryptoAmount, string currencyCode);
        Task<decimal> ConvertUSDToCrypto(decimal usdAmount, string currencyCode);
    }
}
