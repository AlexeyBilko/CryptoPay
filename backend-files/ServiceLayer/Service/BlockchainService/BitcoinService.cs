using Microsoft.Extensions.Configuration;
using NBitcoin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QBitNinja.Client;
using ServiceLayer.DTOs;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Service.BlockchainService
{
    public class BitcoinService : IBitcoinService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string BaseUrlMainnet = "https://api.blockcypher.com/v1/btc/main";
        private const string BaseUrlTestnet = "https://api.blockcypher.com/v1/btc/test3";

        private readonly ICryptographyService _cryptographyService;

        public BitcoinService(HttpClient httpClient, IConfiguration configuration, ICryptographyService cryptographyService)
        {
            _httpClient = httpClient;
            _apiKey = configuration["Blockchain:BitcoinApiKey"];
            _cryptographyService = cryptographyService;
        }

        /// <summary>
        /// Validates the given Bitcoin address on the specified network.
        /// </summary>
        /// <param name="address">The Bitcoin address to validate.</param>
        /// <param name="isTestnet">Flag to determine if the address is on the testnet.</param>
        /// <returns>True if the address is valid on the specified network, otherwise false.</returns>
        public async Task<bool> ValidateAddress(string address, bool isTestnet = true)
        {
            string baseUrl = isTestnet ? BaseUrlTestnet : BaseUrlMainnet;
            string requestUrl = $"{baseUrl}/addrs/{address}/balance?token={_apiKey}";

            HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
            return response.IsSuccessStatusCode;
        }

        /// <summary>
        /// Retrieves the current medium transaction fee per kilobyte.
        /// </summary>
        /// <param name="isTestnet">Flag to specify the network (mainnet or testnet).</param>
        /// <returns>The transaction fee per kilobyte in satoshis.</returns>
        public async Task<decimal> GetTransactionFeeAsync(bool isTestnet = true)
        {
            string feeApiUrl = isTestnet ? "https://testnet-api.smartbit.com.au/v1/blockchain/fees" : "https://api.smartbit.com.au/v1/blockchain/fees";
            HttpResponseMessage response = await _httpClient.GetAsync(feeApiUrl);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                dynamic feeData = JsonConvert.DeserializeObject<dynamic>(content);
                decimal feePerKb = feeData.fees.medium_fee_per_kb;
                return feePerKb;
            }
            throw new Exception("Failed to fetch transaction fees.");
        }

        /// <summary>
        /// Converts Satoshi to Bitcoin.
        /// </summary>
        /// <param name="satoshis">The amount in satoshis.</param>
        /// <returns>The equivalent amount in bitcoins.</returns>
        public decimal SatoshiToBitcoin(long satoshis)
        {
            return satoshis / 100000000m; // Divide by 100 million to convert satoshis to bitcoins
        }

        /// <summary>
        /// Gets the balance of the specified wallet address in bitcoins.
        /// </summary>
        /// <param name="walletAddress">The wallet address to query.</param>
        /// <param name="isTestnet">Whether to check on the testnet or the mainnet.</param>
        /// <returns>The wallet balance in BTC.</returns>
        public async Task<decimal> GetWalletBalanceAsync(string walletAddress, bool isTestnet = true)
        {
            string baseUrl = isTestnet ? BaseUrlTestnet : BaseUrlMainnet;
            string requestUrl = $"{baseUrl}/addrs/{walletAddress}/balance?token={_apiKey}";

            HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                dynamic balanceData = JsonConvert.DeserializeObject<dynamic>(content);
                long finalBalance = balanceData.final_balance;
                return Convert.ToDecimal(finalBalance) / 100000000;
            }
            throw new Exception("Failed to fetch wallet balance.");
        }

        /// <summary>
        /// Sends a transaction from one wallet to another.
        /// </summary>
        /// <param name="fromWallet">The sending wallet's address.</param>
        /// <param name="encryptedPrivateKey">The private key for the sending wallet.</param>
        /// <param name="toWallet">The receiving wallet's address.</param>
        /// <param name="amount">The amount to send in BTC.</param>
        /// <param name="isTestnet">Whether to operate on the testnet or mainnet.</param>
        /// <returns>The transaction hash if the transaction is broadcast successfully.</returns>
        public async Task<string> SendTransactionAsync(string fromWallet, string encryptedPrivateKey, string toWallet, decimal amount, bool isTestnet = false)
        {
            var privateWKey = _cryptographyService.Decrypt(encryptedPrivateKey);

            Network network = isTestnet ? Network.TestNet : Network.Main;
            BitcoinSecret secret = new BitcoinSecret(privateWKey, network);
            BitcoinAddress sourceAddress = secret.GetAddress(ScriptPubKeyType.Legacy);
            BitcoinAddress destinationAddress = BitcoinAddress.Create(toWallet, network);

            var client = new QBitNinjaClient(network);
            var balanceModel = await client.GetBalance(sourceAddress, true);
            var coins = balanceModel.Operations.SelectMany(op => op.ReceivedCoins.Select(coin => coin as Coin));

            if (!coins.Any())
            {
                throw new InvalidOperationException("No spendable coins found in the wallet.");
            }

            TransactionBuilder builder = network.CreateTransactionBuilder(); 
            Money sendAmount = new Money(amount, MoneyUnit.BTC);

            builder.AddCoins(coins);
            builder.AddKeys(secret.PrivateKey);
            builder.Send(destinationAddress, sendAmount);
            builder.SetChange(sourceAddress);
            builder.SendFees(new Money(await GetTransactionFeeAsync(isTestnet), MoneyUnit.Satoshi));

            Transaction tx = builder.BuildTransaction(true);
            return await BroadcastTransaction(tx, isTestnet);
        }

        /// <summary>
        /// Broadcasts a transaction to the Bitcoin network.
        /// </summary>
        /// <param name="tx">The transaction to broadcast.</param>
        /// <param name="isTestnet">Whether to use the testnet or mainnet for broadcasting.</param>
        /// <returns>The transaction hash if the transaction is broadcast successfully.</returns>
        public async Task<string> BroadcastTransaction(Transaction tx, bool isTestnet = false)
        {
            var network = isTestnet ? Network.TestNet : Network.Main;
            var baseUrl = isTestnet ? "https://api.blockcypher.com/v1/btc/test3/txs/push" : "https://api.blockcypher.com/v1/btc/main/txs/push";
            var txHex = tx.ToHex();

            var jsonPayload = new
            {
                tx = txHex
            };

            var content = new StringContent(JsonConvert.SerializeObject(jsonPayload), Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _httpClient.PostAsync(baseUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);
                return result.tx.hash;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to broadcast transaction: {errorContent}");
            }
        }

        /// <summary>
        /// Retrieves a list of recent transactions for a specified wallet address.
        /// </summary>
        /// <param name="walletAddress">The wallet address to query.</param>
        /// <param name="isTestnet">Whether to query the testnet or mainnet.</param>
        /// <returns>A list of transaction details for transactions within the last 4 hours.</returns>
        public async Task<List<TransactionDetails>> GetRecentTransactions(string walletAddress, bool isTestnet)
        {
            string baseUrl = isTestnet ? BaseUrlTestnet : BaseUrlMainnet;
            string requestUrl = $"{baseUrl}/addrs/{walletAddress}/full?token={_apiKey}";

            HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                dynamic transactionsData = JsonConvert.DeserializeObject<dynamic>(content);
                var transactions = new List<TransactionDetails>();

                foreach (var tx in transactionsData.txs)
                {
                    if (tx.inputs.Count == 0 || tx.outputs.Count == 0) continue; // Skip transactions with no inputs or outputs

                    //DateTime txTime;
                    //try
                    //{
                    //    if (tx.received is long)
                    //    {
                    //        txTime = DateTimeOffset.FromUnixTimeSeconds((long)tx.received).DateTime;
                    //    }
                    //    else if (tx.received is DateTime)
                    //    {
                    //        txTime = (DateTime)tx.received;
                    //    }
                    //    else
                    //    {
                    //        continue; // Skip this transaction if received timestamp is not valid
                    //    }
                    //}
                    //catch (Exception)
                    //{
                    //    continue; // Skip this transaction if received timestamp is not valid
                    //}

                    //if (txTime > DateTime.UtcNow.AddHours(-4)) // Checking if the transaction is within the last 4 hours
                    //{
                        string fromAddress = tx.inputs[0].addresses[0];
                        string toAddress = tx.outputs[0].addresses[0];
                        long value = tx.outputs[0].value;

                        if (string.IsNullOrEmpty(fromAddress) || string.IsNullOrEmpty(toAddress) || value == 0)
                        {
                            continue;
                        }

                        transactions.Add(new TransactionDetails
                        {
                            Hash = tx.hash.ToString(),
                            FromAddress = fromAddress,
                            ToAddress = toAddress,
                            Amount = Convert.ToDecimal(value) / 100000000,
                            Timestamp = DateTime.Now
                        });
                    //}
                }

                return transactions;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Failed to fetch recent transactions: {errorContent}");
            }
        }

        public async Task<string> VerifyTransactionByHash(string transactionHash, bool isTestnet = true)
        {
            string baseUrl = isTestnet ? BaseUrlTestnet : BaseUrlMainnet;
            string requestUrl = $"{baseUrl}/txs/{transactionHash}?token={_apiKey}";

            HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
            if (!response.IsSuccessStatusCode)
            {
                return "Failed to fetch transaction data";
            }

            string content = await response.Content.ReadAsStringAsync();
            dynamic txData = JsonConvert.DeserializeObject<dynamic>(content);

            if (txData.error != null)
            {
                return "Failed";
            }

            int confirmations = (int)txData.confirmations;
            if (confirmations > 0)
            {
                if (confirmations >= 6)
                    return "Confirmed";
                else
                    return "Pending 1";
            }
            else
            {
                return "Pending";
            }
        }

        /// <summary>
        /// Verifies if a transaction involving a specified amount between specified wallets has occurred within the last 4 hours.
        /// </summary>
        /// <param name="fromWallet">The sender's wallet address.</param>
        /// <param name="toWallet">The recipient's wallet address.</param>
        /// <param name="amountCrypto">The amount of cryptocurrency sent in the transaction.</param>
        /// <param name="isTestnet">Whether to check in the testnet or mainnet.</param>
        /// <returns>True if the transaction is found, otherwise false.</returns>
        public async Task<(bool, PaymentPageTransactionDTO)> VerifyTransaction(string fromWallet, string toWallet, decimal amountCrypto, bool isTestnet, bool isDonation)
        {
            try
            {
                var recentTransactions = await GetRecentTransactions(toWallet, isTestnet);
                var transaction = recentTransactions.FirstOrDefault(tx =>
                    tx.FromAddress == fromWallet &&
                    tx.ToAddress == toWallet &&
                    (isDonation || Math.Abs(tx.Amount - amountCrypto) < 0.0001M));

                if (transaction != null)
                {
                    var transactionDetails = new PaymentPageTransactionDTO
                    {
                        TransactionHash = transaction.Hash,
                        SenderWalletAddress = transaction.FromAddress,
                        Status = "successful",
                        CreatedAt = DateTime.Now,
                        BlockNumber = 0,
                        BlockTimestamp = transaction.Timestamp,
                        TransactionIndex = "0",
                        GasPrice = 0,
                        GasUsed = 0,
                        InputData = transaction.Hash,
                        TransactionFee = await GetTransactionFeeAsync(true),
                        ActualAmountCrypto = transaction.Amount
                    };
                    return (true, transactionDetails);
                }
                return (false, null);
            }
            catch(Exception ex)
            {
                return (false, null);
            }
        }

        /// <summary>
        /// Recovers a wallet address from a private key.
        /// </summary>
        /// <param name="privateKey">The private key to recover the wallet from.</param>
        /// <param name="isTestnet">Whether to use the testnet or mainnet for the recovery.</param>
        /// <returns>The wallet address associated with the private key.</returns>
        public Task<string> RecoverWallet(string privateKey, bool isTestnet)
        {
            Network network = isTestnet ? Network.TestNet : Network.Main;
            BitcoinSecret secret = new BitcoinSecret(privateKey, network);
            return Task.FromResult(secret.GetAddress(ScriptPubKeyType.Legacy).ToString());
        }

        /// <summary>
        /// Gets the current market price of Bitcoin in USD.
        /// </summary>
        /// <returns>The current price of Bitcoin in USD.</returns>
        public async Task<decimal> GetCurrentPriceAsync()
        {
            string requestUrl = "https://api.coingecko.com/api/v3/simple/price?ids=bitcoin&vs_currencies=usd";

            HttpResponseMessage response = await _httpClient.GetAsync(requestUrl);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                dynamic priceData = JsonConvert.DeserializeObject<dynamic>(content);
                decimal currentPrice = priceData.bitcoin.usd;
                return currentPrice;
            }
            else
            {
                throw new Exception("Failed to fetch current price.");
            }
        }

        public async Task<decimal> GetBitcoinPriceInUSD()
        {
            string apiUrl = "https://blockchain.info/ticker"; // Blockchain.info provides a simple API for BTC prices
            var response = await _httpClient.GetStringAsync(apiUrl);
            var json = JObject.Parse(response);
            return json["USD"]["last"].Value<decimal>();
        }

        public async Task<(string address, string encryptedPrivateKey)> CreateBitcoinWalletAsync(bool isTestnet = true)
        {
            var network = isTestnet ? Network.TestNet : Network.Main;
            // Generate a new Bitcoin address and private key
            var privateKey = new Key(); // Generate a random private key
            var address = privateKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, network).ToString();

            // Encrypt the private key
            var encryptedPrivateKey = _cryptographyService.Encrypt(privateKey.ToString(network));
            //var encryptedPrivateKey = privateKey.ToString(network);

            return (address, encryptedPrivateKey);
        }
    }
}
