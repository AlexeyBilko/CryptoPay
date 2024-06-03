using Microsoft.Extensions.Configuration;
using NBitcoin;
using Newtonsoft.Json;
using QBitNinja.Client;
using ServiceLayer.DTOs;
using System.Text;

namespace ServiceLayer.Service.BlockchainService
{
    public interface IBitcoinService
    {
        Task<bool> ValidateAddress(string address, bool isTestnet);
        Task<decimal> GetTransactionFeeAsync(bool isTestnet);
        Task<decimal> GetWalletBalanceAsync(string walletAddress, bool isTestnet);
        Task<string> SendTransactionAsync(string fromWallet, string encryptedPrivateKey, string toWallet, decimal amount, bool isTestnet);
        Task<string> BroadcastTransaction(Transaction tx, bool isTestnet);
        Task<List<TransactionDetails>> GetRecentTransactions(string walletAddress, bool isTestnet);
        Task<string> VerifyTransactionByHash(string transactionHash, bool isTestnet);
        Task<(bool, PaymentPageTransactionDTO)> VerifyTransaction(string fromWallet, string toWallet, decimal amountCrypto, bool isTestnet, bool isDonation);
        Task<string> RecoverWallet(string privateKey, bool isTestnet);
        Task<decimal> GetCurrentPriceAsync();
        Task<decimal> GetBitcoinPriceInUSD();
        Task<(string address, string encryptedPrivateKey)> CreateBitcoinWalletAsync(bool isTestnet);
    }
}
