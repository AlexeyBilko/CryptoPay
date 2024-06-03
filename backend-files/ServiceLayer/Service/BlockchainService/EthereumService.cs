using Microsoft.Extensions.Configuration;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServiceLayer.DTOs;
using System.Linq;
using System.Numerics;

namespace ServiceLayer.Service.BlockchainService
{
    public class EthereumService : IEthereumService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private Web3 _web3;
        private readonly string _etherscanApiKey;
        private readonly string _baseUrlMainnet = "https://api.etherscan.io/api";
        private readonly string _baseUrlSepolia = "https://api-sepolia.etherscan.io/api";//"https://api-goerli.etherscan.io/api";
        private readonly ICryptographyService _cryptographyService;

        public EthereumService(HttpClient httpClient, IConfiguration configuration, ICryptographyService cryptographyService)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _etherscanApiKey = configuration["Blockchain:EthereumApiKey"];
            _cryptographyService = cryptographyService;
            InitializeWeb3();
        }

        /// <summary>
        /// Initializes the Web3 client with appropriate network settings based on configuration.
        /// </summary>
        private void InitializeWeb3()
        {
            string infuraProjectId = _configuration["Blockchain:InfuraProjectId"];

            string infuraUrl = _configuration["Blockchain:UseTestnet"] == "true" ?
                    $"https://sepolia.infura.io/v3/{infuraProjectId}" :
                    $"https://mainnet.infura.io/v3/{infuraProjectId}";

            _web3 = new Web3(infuraUrl);
        }

        /// <summary>
        /// Validates a given Ethereum address by checking its balance on the blockchain to ensure it is active.
        /// </summary>
        /// <param name="address">The Ethereum address to validate.</param>
        /// <param name="isTestnet">Flag indicating whether to use the testnet.</param>
        /// <returns>True if the address is valid, otherwise false.</returns>
        public async Task<bool> ValidateAddress(string address, bool isTestnet)
        {
            try
            {
                string baseUrl = isTestnet ? _baseUrlSepolia : _baseUrlMainnet;
                string url = $"{baseUrl}?module=account&action=balance&address={address}&tag=latest&apikey={_etherscanApiKey}";

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to validate address due to API error.");
                }
                var content = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject<dynamic>(content);
                return result.status == "1";
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to validate address.", ex);
            }
        }

        /// <summary>
        /// Retrieves the current average transaction fee in GWei.
        /// </summary>
        /// <param name="isTestnet">Indicates whether to retrieve the fee for the testnet or mainnet.</param>
        /// <returns>The current transaction fee in GWei.</returns>
        public async Task<decimal> GetTransactionFeeAsync(bool isTestnet)
        {
            try
            {
                string baseUrl = isTestnet ? _baseUrlSepolia : _baseUrlMainnet;
                string url = $"{baseUrl}?module=proxy&action=eth_gasPrice&apikey={_etherscanApiKey}";

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to fetch transaction fees due to API error.");
                }
                var content = await response.Content.ReadAsStringAsync();
                dynamic gasPrice = JsonConvert.DeserializeObject<dynamic>(content);
                BigInteger price = BigInteger.Parse((string)gasPrice.result);
                return Web3.Convert.FromWei(price, 9); // Convert from Wei to GWei
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to fetch transaction fees.", ex);
            }
        }

        /// <summary>
        /// Retrieves the balance of the specified Ethereum wallet.
        /// </summary>
        /// <param name="walletAddress">The address of the wallet to check.</param>
        /// <param name="isTestnet">Specifies whether the wallet is on the testnet or mainnet.</param>
        /// <returns>The balance of the wallet in Ether.</returns>
        public async Task<decimal> GetWalletBalanceAsync(string walletAddress, bool isTestnet)
        {
            try
            {
                var balance = await _web3.Eth.GetBalance.SendRequestAsync(walletAddress);
                return Web3.Convert.FromWei(balance.Value);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to fetch wallet balance for {walletAddress}.", ex);
            }
        }

        /// <summary>
        /// Sends Ethereum from one wallet to another.
        /// </summary>
        /// <param name="fromWallet">The address of the sender's wallet.</param>
        /// <param name="toWallet">The address of the recipient's wallet.</param>
        /// <param name="amount">The amount of Ether to send.</param>
        /// <param name="privateKey">The private key of the sender's wallet.</param>
        /// <param name="isTestnet">Indicates whether the transaction should occur on the testnet or mainnet.</param>
        /// <returns>The transaction hash if the transaction is successful.</returns>
        public async Task<string> SendTransactionAsync(string fromWallet, string encryptedPrivateKey, string toWallet, decimal amount, bool isTestnet = false)
        {
            try
            {
                // Decrypt the private key (assuming you have a decryption method in your cryptography service)
                //var privateWKey = _cryptographyService.Decrypt(encryptedPrivateKey);
                var privateWKey = encryptedPrivateKey; // Use this line if the key is already decrypted

                // Initialize the account and Web3 instance
                var account = new Nethereum.Web3.Accounts.Account(privateWKey);

                string infuraProjectId = _configuration["Blockchain:InfuraProjectId"];

                string infuraUrl = _configuration.GetValue<bool>("Blockchain:UseTestnet") ?
                        $"https://sepolia.infura.io/v3/{infuraProjectId}" :
                        $"https://mainnet.infura.io/v3/{infuraProjectId}";

                _web3 = new Web3(account, infuraUrl);
                //_web3 = new Web3(account, isTestnet ? _baseUrlSepolia : _baseUrlMainnet);

                // Get the transaction count (nonce) for the account
                var nonce = await _web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(fromWallet, Nethereum.RPC.Eth.DTOs.BlockParameter.CreatePending());

                // Create the transaction input
                var transactionInput = new Nethereum.RPC.Eth.DTOs.TransactionInput()
                {
                    From = fromWallet,
                    To = toWallet,
                    Value = new HexBigInteger(Web3.Convert.ToWei(amount)),
                    Nonce = nonce
                };

                // Send the transaction and wait for the receipt
                var transactionReceipt = await _web3.TransactionManager.SendTransactionAndWaitForReceiptAsync(transactionInput);

                return transactionReceipt.TransactionHash;
            }
            catch (Nethereum.JsonRpc.Client.RpcClientUnknownException rpcEx)
            {
                throw new Exception($"RPC error occurred: {rpcEx.Message}", rpcEx);
            }
            catch (JsonSerializationException jsonEx)
            {
                throw new Exception($"JSON serialization error: {jsonEx.Message}", jsonEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to send transaction.", ex);
            }
        }


        /// <summary>
        /// Retrieves a list of recent transactions for a specified wallet address within the last 4 hours.
        /// </summary>
        /// <param name="walletAddress">The wallet address to query.</param>
        /// <param name="isTestnet">Specifies whether to query the testnet or mainnet.</param>
        /// <returns>A list of transactions that occurred within the last 4 hours.</returns>
        public async Task<List<TransactionDetails>> GetRecentTransactions(string walletAddress, bool isTestnet)
        {
            try
            {
                // Get the current block number first
                string baseUrl = isTestnet ? _baseUrlSepolia : _baseUrlMainnet;
                string currentBlockUrl = $"{baseUrl}?module=proxy&action=eth_blockNumber&apikey={_etherscanApiKey}";
                HttpResponseMessage blockResponse = await _httpClient.GetAsync(currentBlockUrl);
                if (!blockResponse.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to fetch current block number.");
                }
                string blockContent = await blockResponse.Content.ReadAsStringAsync();
                dynamic blockData = JsonConvert.DeserializeObject<dynamic>(blockContent);
                string currentBlockHex = (string)blockData.result;
                long currentBlock = Convert.ToInt64(currentBlockHex, 16);

                // Estimate blocks for the last 4 hours
                BigInteger blocksInFourHours = new BigInteger(4 * 3600 / 15); // 4 hours, 3600 seconds per hour, approximately 15 seconds per block
                BigInteger startBlock = currentBlock - blocksInFourHours;

                // Use the estimated block range to query transactions
                string url = $"{baseUrl}?module=account&action=txlist&address={walletAddress}&startblock={startBlock}&endblock={currentBlock}&sort=desc&apikey={_etherscanApiKey}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"Failed to fetch recent transactions: {errorContent}");
                }
                string content = await response.Content.ReadAsStringAsync();
                return ParseTransactionResponse(content);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to retrieve recent transactions.", ex);
            }
        }

        public async Task<string> VerifyTransactionByHash(string transactionHash, bool isTestnet = false)
        {
            string baseUrl = isTestnet ? _baseUrlSepolia : _baseUrlMainnet;
            string url = $"{baseUrl}?module=transaction&action=gettxreceiptstatus&txhash={transactionHash}&apikey={_etherscanApiKey}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                dynamic result = JsonConvert.DeserializeObject<dynamic>(content);
                if (result.status == "1" && result.result.status == "1")
                {
                    return "Confirmed";
                }
                else if (result.status == "1")
                    return "Pending";
                else
                    return "Failed";
            }
            else
            {
                return "Failed";
            }
        }

        /// <summary>
        /// Verifies if a transaction of a specified amount occurred between two wallets within the last 4 hours.
        /// </summary>
        /// <param name="fromWallet">The sender's wallet address.</param>
        /// <param name="toWallet">The recipient's wallet address.</param>
        /// <param name="amountCrypto">The amount of Ether sent in the transaction.</param>
        /// <param name="isTestnet">Indicates whether to verify the transaction on the testnet or mainnet.</param>
        /// <returns>True if such a transaction is found, otherwise false.</returns>
        public async Task<(bool, PaymentPageTransactionDTO)> VerifyTransaction(string fromWallet, string toWallet, decimal amountCrypto, bool isTestnet, bool isDonation)
        {
            try
            {
                var recentTransactions = await GetRecentTransactions(toWallet, isTestnet);
                var transaction = recentTransactions.FirstOrDefault(tx =>
                            tx.FromAddress.ToLower() == fromWallet.ToLower() &&
                            tx.ToAddress.ToLower() == toWallet.ToLower() &&
                            (isDonation || Math.Abs(tx.Amount - amountCrypto) < 0.0001M)); // Small tolerance for floating-point comparison

                if (transaction != null)
                {
                    var transactionDetails = new PaymentPageTransactionDTO
                    {
                        TransactionHash = transaction.Hash,
                        SenderWalletAddress = transaction.FromAddress,
                        Status = "successful",
                        CreatedAt = DateTime.Now,
                        BlockNumber = 0, // replace with actual block number
                        BlockTimestamp = transaction.Timestamp, // replace with actual block timestamp
                        TransactionIndex = "0", // replace with actual transaction index
                        GasPrice = 0, // replace with actual gas price
                        GasUsed = 0, // replace with actual gas used
                        InputData = transaction.Hash, // replace with actual input data
                        TransactionFee = 0, // replace with actual transaction fee
                        ActualAmountCrypto = transaction.Amount
                    };
                    return (true, transactionDetails);
                }
                return (false, null);
            }
            catch (Exception ex)
            {
                return (false, null);
                //throw new Exception("Failed to verify transaction.", ex);
            }
        }

        /// <summary>
        /// Recovers a wallet address from a private key.
        /// </summary>
        /// <param name="privateKey">The private key for which to recover the address.</param>
        /// <param name="isTestnet">Indicates whether the address should be recovered for the testnet or mainnet.</param>
        /// <returns>The Ethereum wallet address associated with the private key.</returns>
        public Task<string> RecoverWallet(string privateKey, bool isTestnet)
        {
            try
            {
                var account = new Nethereum.Web3.Accounts.Account(privateKey);
                return Task.FromResult(account.Address);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to recover wallet.", ex);
            }
        }

        /// <summary>
        /// Retrieves the current market price of Ethereum in USD.
        /// </summary>
        /// <returns>The current price of Ethereum in USD.</returns>
        public async Task<decimal> GetCurrentPriceAsync()
        {
            try
            {
                string url = "https://api.coingecko.com/api/v3/simple/price?ids=ethereum&vs_currencies=usd";
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Failed to fetch current price.");
                }
                string content = await response.Content.ReadAsStringAsync();
                dynamic priceData = JsonConvert.DeserializeObject<dynamic>(content);
                return priceData.ethereum.usd;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to fetch current price.", ex);
            }
        }

        /// <summary>
        /// Parses the JSON response from the Ethereum blockchain API to extract transaction details.
        /// </summary>
        /// <param name="jsonContent">The JSON content containing transaction data.</param>
        /// <returns>A list of transaction details extracted from the JSON content.</returns>
        private List<TransactionDetails> ParseTransactionResponse(string jsonContent)
        {
            dynamic data = JsonConvert.DeserializeObject<dynamic>(jsonContent);
            var transactions = new List<TransactionDetails>();
            foreach (var tx in data.result)
            {
                DateTime txTime = DateTimeOffset.FromUnixTimeSeconds((long)tx.timeStamp).UtcDateTime;
                if ((DateTime.UtcNow - txTime).TotalHours <= 4)
                {
                    transactions.Add(new TransactionDetails
                    {
                        Hash = tx.hash,
                        FromAddress = tx.from,
                        ToAddress = tx.to,
                        Amount = Web3.Convert.FromWei(BigInteger.Parse(tx.value.ToString())),
                        Timestamp = txTime
                    });
                }
            }
            return transactions;
        }

        public async Task<decimal> GetEthereumPriceInUSD()
        {
            string apiUrl = "https://api.coinbase.com/v2/exchange-rates?currency=ETH";
            var response = await _httpClient.GetStringAsync(apiUrl);
            var json = JObject.Parse(response);
            var usdPrice = json["data"]["rates"]["USD"].Value<decimal>();
            return usdPrice;
        }

        public async Task<(string address, string encryptedPrivateKey)> CreateEthereumWalletAsync()
        {
            // Generate a new Ethereum account
            var ecKey = Nethereum.Signer.EthECKey.GenerateKey();
            var privateKey = ecKey.GetPrivateKey();
            var address = ecKey.GetPublicAddress();

            // Encrypt the private key
            var encryptedPrivateKey = _cryptographyService.Encrypt(privateKey);
            //var encryptedPrivateKey = privateKey;

            //var web3 = new Web3(_baseUrlGoerli);
            //var balance = await web3.Eth.GetBalance.SendRequestAsync(address);

            return (address, encryptedPrivateKey);
        }


    }
}
