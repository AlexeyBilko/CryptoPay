using Microsoft.AspNetCore.Mvc;
using ServiceLayer.DTOs;
using ServiceLayer.Service.Abstraction;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System;
using ServiceLayer.Service.Realization.IdentityServices;
using ServiceLayer.Service.Realization;
using ServiceLayer.Service.BlockchainService;

namespace CryptoPaymentGateway.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EarningsController : ControllerBase
    {
        private const bool TESTNET = true;

        private readonly IEarningsService _earningsService;
        private readonly IWithdrawalService _withdrawalService;
        private readonly IPaymentPageService _paymentPageService;
        private readonly IPaymentPageTransactionService _paymentPageTransactionService;
        private readonly IAmountDetailsService _amountDetailsService;
        private readonly ICurrencyService _currencyService;
        private readonly ISystemWalletService _systemWalletService;
        private readonly IUserWalletService _userWalletService;
        private readonly IBitcoinService _bitcoinService;
        private readonly IEthereumService _ethereumService;
        private readonly UserService _userService;
        private readonly EmailService _emailService;

        public EarningsController(IEarningsService earningsService,
            IWithdrawalService withdrawalService,
            IPaymentPageService paymentPageService,
            IPaymentPageTransactionService paymentPageTransactionService,
            IAmountDetailsService amountDetailsService,
            ICurrencyService currencyService,
            ISystemWalletService systemWalletService,
            IBitcoinService bitcoinService,
            IEthereumService ethereumService,
            IUserWalletService userWalletService,
            UserService userService,
            EmailService emailService)
        {
            _earningsService = earningsService;
            _withdrawalService = withdrawalService;
            _paymentPageService = paymentPageService;
            _paymentPageTransactionService = paymentPageTransactionService;
            _amountDetailsService = amountDetailsService;
            _currencyService = currencyService;
            _systemWalletService = systemWalletService;
            _userWalletService = userWalletService;
            _bitcoinService = bitcoinService;
            _ethereumService = ethereumService;
            _userService = userService;
            _emailService = emailService;
        }

        /// <summary>
        /// Enables users to view their total earnings in USD and cryptocurrencies.
        /// </summary>
        [HttpGet("view-earnings")]
        public async Task<IActionResult> ViewEarnings()
        {
            try
            {
                // Retrieve earnings in BTC and ETH
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not found or unauthorized.");
                }
                var earnings = await _earningsService.GetEarningsByUserId(userId);
                if (earnings == null)
                {
                    return NotFound("Earnings information not found.");
                }

                // Get current exchange rates
                //decimal btcToUsd = await _currencyService.GetCurrentPriceAsync("bitcoin");
                //decimal ethToUsd = await _currencyService.GetCurrentPriceAsync("ethereum");

                // Calculate total earnings in USD
                //decimal totalEarningsInUsd = (earnings.TotalEarnedBTC * btcToUsd) + (earnings.TotalEarnedETH * ethToUsd);

                //if(earnings.TotalEarnedUSD != totalEarningsInUsd)
                //{
                //    earnings.TotalEarnedUSD = totalEarningsInUsd;

                //    earnings = await _earningsService.UpdateAsync(earnings);
                //}

                // Prepare response
                //var earningsResponse = new
                //{
                //    TotalEarningsBtc = earnings.TotalEarnedBTC,
                //    TotalEarningsEth = earnings.TotalEarnedETH,
                //    TotalEarningsUsd = totalEarningsInUsd
                //};

                return Ok(earnings);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Failed to retrieve earnings.", Exception = ex.Message });
            }
        }

        /// <summary>
        /// Allows users to withdraw their earnings in a specified cryptocurrency to a designated wallet.
        /// </summary>
        [HttpPost("withdraw-earnings")]
        public async Task<IActionResult> WithdrawEarnings([FromBody] WithdrawalViewModel model)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not found or unauthorized.");
                }


                var currency = await _currencyService.GetByCurrencyCode(model.CurrencyCode);
                if (currency == null)
                {
                    return NotFound("Currency is not supported or not exist.");
                }

                var earnings = (await _earningsService.GetAllAsync()).Where(e => e.UserId == userId).First();
                if (earnings == null)
                {
                    return NotFound("Earnings information not found.");
                }

                var currentPrice = await _currencyService.GetCurrentPriceAsync(model.CurrencyCode);
                var amountUSD = currentPrice * model.Amount;
                var amountDetails = await _amountDetailsService.AddAsync(new AmountDetailsDTO()
                {
                    CurrencyId = currency.Id,
                    AmountCrypto = model.Amount,
                    AmountUSD = amountUSD
                });

                var userWallet = await _userWalletService.GetByWalletAddress(model.WalletNumber);
                if (userWallet == null)
                {
                    userWallet = await _userWalletService.AddAsync(new UserWalletDTO()
                    {
                        WalletNumber = model.WalletNumber,
                        UserId = userId
                    });
                }

                bool sufficientFunds = (currency.CurrencyCode.ToLower() == "btc" && earnings.CurrentBalanceBTC >= amountDetails.AmountCrypto) ||
                                       (currency.CurrencyCode.ToLower() == "eth" && earnings.CurrentBalanceETH >= amountDetails.AmountCrypto);

                if (!sufficientFunds)
                {
                    return BadRequest("Insufficient funds for this withdrawal.");
                }

                var systemWallet = await _systemWalletService.GetSuitableWalletForCurrency(model.CurrencyCode);
                if (systemWallet == null)
                {
                    return NotFound("Suitable system wallet not found.");
                }

                string transactionResult = null;
                if (currency.CurrencyCode == "BTC")
                {
                    transactionResult = await _bitcoinService.SendTransactionAsync(
                        systemWallet.WalletNumber,
                        systemWallet.EncryptedWalletCodePhrase,
                        userWallet.WalletNumber,
                        amountDetails.AmountCrypto,
                        TESTNET);
                }
                else if (currency.CurrencyCode == "ETH")
                {
                    transactionResult = await _ethereumService.SendTransactionAsync(
                        systemWallet.WalletNumber,
                        systemWallet.EncryptedWalletCodePhrase,
                        userWallet.WalletNumber,
                        amountDetails.AmountCrypto,
                        TESTNET);
                }
                else
                {
                    return NotFound("SendTransactionAsync failed - Currency is not supported or not exist.");
                }

                if (string.IsNullOrEmpty(transactionResult))
                {
                    return BadRequest("Failed to process the withdrawal transaction.");
                }

                if (model.CurrencyCode == "BTC")
                {
                    earnings.CurrentBalanceBTC -= amountDetails.AmountCrypto;
                    earnings.CurrentBalanceUSD -= amountUSD;
                }
                else
                {
                    earnings.CurrentBalanceETH -= amountDetails.AmountCrypto;
                    earnings.CurrentBalanceUSD -= amountUSD;
                }

                await _earningsService.UpdateAsync(earnings);

                await _withdrawalService.AddAsync(new WithdrawalDTO
                {
                    SystemWalletId = systemWallet.Id,
                    UserWalletId = userWallet.Id,
                    AmountDetailsId = amountDetails.Id,
                    Status = "Initiated - Pending",
                    TransactionId = transactionResult,
                    RequestedDate = DateTime.Now,
                    CompletedDate = null
                });

                var message = $"Розпочато виведення коштів. Криптовалюта: {currency.CurrencyCode}; Кількість Криптовалюти: {amountDetails.AmountCrypto}; До гаманця (Ваш гаманець): {userWallet.WalletNumber}; Хеш транзакції:  {transactionResult}.";
                var userEmail = (await _userService.FindByIdAsync(userId)).Email;
                await _emailService.SendEmailAsync(userEmail, "Розпочато виведення коштів", message);

                return Ok(new { Message = "Withdrawal iniated", TransactionId = transactionResult });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "An error occurred while processing withdrawal request.", Exception = ex.Message });
            }
        }

        [HttpGet("check-transaction-status/{transactionId}")]
        public async Task<IActionResult> CheckTransactionStatus(int transactionId)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not found or unauthorized.");
            }

            var withdrawalTransaction = await _withdrawalService.GetAsync(transactionId);
            if (withdrawalTransaction == null || withdrawalTransaction.UserWallet.UserId != userId)
            {
                return NotFound("Transaction not found or access denied.");
            }

            string confirmations = "Pending";

            if (withdrawalTransaction.AmountDetails.Currency.CurrencyCode == "BTC")
            {
                confirmations = await _bitcoinService.VerifyTransactionByHash(withdrawalTransaction.TransactionId, TESTNET);
            }
            else if (withdrawalTransaction.AmountDetails.Currency.CurrencyCode == "ETH")
            {
                confirmations = await _ethereumService.VerifyTransactionByHash(withdrawalTransaction.TransactionId, TESTNET);
            }
            else
            {
                return NotFound("SendTransactionAsync failed - Currency is not supported or not exist.");
            }

            if (confirmations == "Confirmed")
            {
                withdrawalTransaction.Status = "Confirmed";
                withdrawalTransaction.CompletedDate = DateTime.Now;
                await _withdrawalService.UpdateAsync(withdrawalTransaction);

                var message = $"Withdrawal successful. Amount Crypto: {withdrawalTransaction.AmountDetails.AmountCrypto}; To wallet: {withdrawalTransaction.UserWallet.WalletNumber}; Transaction Hash:  {withdrawalTransaction.TransactionId}.";
                var userEmail = (await _userService.FindByIdAsync(userId)).Email;
                await _emailService.SendEmailAsync(userEmail, "Withdrawal Successful", message);

                return Ok(new { Status = "Confirmed", Message = "Transaction confirmed successfully." });
            }
            else if (confirmations == "Failed")
            {
                withdrawalTransaction.Status = "Failed";
                await _withdrawalService.UpdateAsync(withdrawalTransaction);

                var earnings = (await _earningsService.GetAllAsync()).Where(e => e.UserId == userId).First();
                if (earnings == null)
                {
                    return NotFound("Earnings information not found.");
                }

                //if transaction failed, we should return user's earnings back to balance
                var currentPrice = await _currencyService.GetCurrentPriceAsync(withdrawalTransaction.AmountDetails.Currency.CurrencyCode);
                var amountUSD = currentPrice * withdrawalTransaction.AmountDetails.AmountCrypto;

                if (withdrawalTransaction.AmountDetails.Currency.CurrencyCode == "BTC")
                {
                    earnings.CurrentBalanceBTC += withdrawalTransaction.AmountDetails.AmountCrypto;
                    earnings.CurrentBalanceUSD += amountUSD;
                }
                else
                {
                    earnings.CurrentBalanceETH += withdrawalTransaction.AmountDetails.AmountCrypto;
                    earnings.CurrentBalanceUSD += amountUSD;
                }

                await _earningsService.UpdateAsync(earnings);
                return Ok(new { Status = "Failed", CompletedDate = withdrawalTransaction.CompletedDate, Message = "Transaction Failed." });
            }
            else if (confirmations == "Pending 1")
            {
                return Ok(new { Status = "Pending", Message = "Transaction is still pending (1+ confirmation)" });
            }

            return Ok(new { Status = "Pending", Message = "Transaction is still pending." });
        }

        /// <summary>
        /// Generates a detailed PDF report of earnings within a specified date range.
        /// </summary>
        [HttpGet("generate-earnings-report")]
        public async Task<IActionResult> GenerateEarningsReport(DateTime startDate, DateTime endDate)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized("User not found or unauthorized.");

                var paymentPages = await _paymentPageService.GetAllAsync();
                var transactions = await _paymentPageTransactionService.GetTransactionsForReportAsync(userId, startDate, endDate);
                var withdrawals = await _withdrawalService.GetWithdrawalsForReportAsync(userId, startDate, endDate);
                var earnings = await _earningsService.GetEarningsForReportAsync(userId, startDate, endDate);


                var pdfBytes = PDFHelper.CreateEarningsReportPdf(paymentPages.ToList(), transactions, withdrawals, earnings, startDate, endDate);

                var userEmail = (await _userService.FindByIdAsync(userId)).Email;
                if (!string.IsNullOrEmpty(userEmail))
                {
                    var subject = "Ваш звіт про доходи";
                    var body = "До листа додається звіт про ваші прибутки за вказаний період.";
                    await _emailService.SendEmailWithPDFAsync(userEmail, subject, body, pdfBytes, "EarningsReport.pdf");
                }

                return File(pdfBytes, "application/pdf", "EarningsReport.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Failed to generate report.", Exception = ex.Message });
            }
        }

        /// <summary>
        /// Allows users to view the history of their withdrawals.
        /// </summary>
        [HttpGet("view-withdrawal-history")]
        public async Task<IActionResult> ViewWithdrawalHistory()
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not found or unauthorized.");
                }

                var withdrawals = (await _withdrawalService.GetAllAsync()).Where(w => w.UserWallet.UserId == userId);
                if (withdrawals.Any())
                {
                    return Ok(withdrawals);
                }
                return NotFound("No withdrawal history found.");
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Failed to retrieve withdrawal history.", Exception = ex.Message });
            }
        }
    }

    public class WithdrawalViewModel
    {
        public string WalletNumber { get; set; } // Address of the user's wallet
        public decimal Amount { get; set; }  // Amount to withdraw
        public string CurrencyCode { get; set; }  // "BTC" or "ETH"
    }
}
