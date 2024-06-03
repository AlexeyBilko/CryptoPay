using Microsoft.AspNetCore.Mvc;
using ServiceLayer.DTOs;
using ServiceLayer.Service.Abstraction;
using System.Security.Claims;
using System.Linq;
using System;
using ServiceLayer.Service.BlockchainService;
using ServiceLayer.Service.Realization;
using Microsoft.AspNetCore.Authorization;
using ServiceLayer.Service.Realization.IdentityServices;

namespace CryptoPaymentGateway.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        //private readonly SystemWalletService _systemWalletService;
        //private readonly UserWalletService _userWalletService;
        private readonly IPaymentPageTransactionService _transactionService;
        private readonly IEarningsService _earningsService;
        private readonly IPaymentPageService _paymentPageService;
        private readonly IBitcoinService _bitcoinService;
        private readonly IEthereumService _ethereumService;
        private readonly UserService _userService;
        private readonly EmailService _emailService;

        public TransactionController(
            //SystemWalletService systemWalletService,
            //UserWalletService userWalletService,
            IPaymentPageTransactionService transactionService,
            IEarningsService earningsService,
            IPaymentPageService paymentPageService,
            IBitcoinService bitcoinService,
            IEthereumService ethereumService,
            UserService userService,
            EmailService emailService)
        {
            //_systemWalletService = systemWalletService;
            //_userWalletService = userWalletService;
            _transactionService = transactionService;
            _earningsService = earningsService;
            _paymentPageService = paymentPageService;
            _bitcoinService = bitcoinService;
            _ethereumService = ethereumService;
            _userService = userService;
            _emailService = emailService;
        }

        /// <summary>
        /// Verifies if a specified transaction occurred.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("verify-tr")]
        public async Task<IActionResult> VerifyTransaction([FromBody] VerifyTransactionRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.SenderEmail))
                {
                    return BadRequest("Sender email is required for donations.");
                }

                (bool verified, PaymentPageTransactionDTO transactionDetails) = (false, null);

                if (request.Type == "btc")
                {
                    //(verified, transactionDetails) = await _bitcoinService.VerifyTransaction(request.FromWallet, request.ToWallet, request.AmountCrypto, request.IsTestnet, request.IsDonation);
                    (verified, transactionDetails) = await _bitcoinService.VerifyTransaction(request.FromWallet, "tb1pr3acewa99mdrpy2r4k2x6wqxaqlzec9hglh8nmvtpkq36cgmp2qsvl8hu4", (decimal)0.00021933, true, false);

                }
                else if (request.Type == "eth")
                {
                    (verified, transactionDetails) = await _ethereumService.VerifyTransaction(request.FromWallet, request.ToWallet, request.AmountCrypto, request.IsTestnet, request.IsDonation);
                }

                if (verified)
                {
                    transactionDetails.PaymentPageId = request.PageId;
                    transactionDetails.PayerEmail = request.SenderEmail;

                    var transaction = await _transactionService.AddAsync(transactionDetails);

                    var userId = transaction.PaymentPage.UserId;
                    var userEmail = (await _userService.FindByIdAsync(userId)).Email;

                    var earnings = await _earningsService.GetEarningsByUserId(userId);

                    if (request.Type == "eth")
                    {
                        earnings.CurrentBalanceETH += request.AmountCrypto;
                        earnings.TotalEarnedETH += request.AmountCrypto;
                    }
                    else if (request.Type == "btc")
                    {
                        earnings.CurrentBalanceBTC += request.AmountCrypto;
                        earnings.TotalEarnedBTC += request.AmountCrypto;
                    }

                    await _earningsService.UpdateAsync(earnings);

                    var paymentPage = await _paymentPageService.GetAsync(request.PageId);
                    var paymentPageDetails = $"Payment Page ID: {paymentPage.Id}, Title: {paymentPage.Title}, Amount: {request.AmountCrypto} {request.Type.ToUpper()}";

                    var userSubject = $"Новий успішний платіж на вашій платіжній сторінці {paymentPage.Id}";
                    var userBody = $"Новий платіж успішно оброблено на вашій платіжній сторінці.<br><br>{paymentPageDetails}<br><br>Електронна пошта платника: {request.SenderEmail}";

                    var senderSubject = "Ваш платіж пройшов успішно";
                    var senderBody = $"Ваш платіж успішно оброблено на платіжній сторінці.<br><br>{paymentPageDetails}<br><br>Електронна пошта власника платіжної сторінки: {userEmail}";

                    await _emailService.SendEmailAsync(userEmail, userSubject, userBody);
                    await _emailService.SendEmailAsync(request.SenderEmail, senderSubject, senderBody);


                    return Ok(new { status = "successful", transaction });
                }
                return Ok(new { status = "not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        public class VerifyTransactionRequest
        {
            public int PageId { get; set; }
            public string Type { get; set; }
            public string FromWallet { get; set; }
            public string ToWallet { get; set; }
            public decimal AmountCrypto { get; set; }
            public string SenderEmail { get; set; }
            public bool IsTestnet { get; set; }
            public bool IsDonation { get; set; }
        }



        /// <summary>
        /// Gets a specific transaction by its ID.
        /// </summary>
        /// <param name="transactionId">The ID of the transaction.</param>
        /// <returns>The transaction details if found, or an error message if not.</returns>
        [HttpGet("{transactionId}")]
        public async Task<IActionResult> GetTransactionById(int transactionId)
        {
            try
            {
                var transaction = await _transactionService.GetAsync(transactionId);
                if (transaction == null)
                {
                    return NotFound("Transaction not found.");
                }
                return Ok(transaction);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all transactions for a specific payment page.
        /// </summary>
        /// <param name="pageId">The ID of the payment page.</param>
        /// <returns>A list of transactions associated with the specified payment page.</returns>
        [HttpGet("bypage/{pageId}")]
        public async Task<IActionResult> GetTransactionsByPageId(int pageId)
        {
            try
            {
                var allTransactions = await _transactionService.GetAllAsync();
                var transactions = allTransactions.Where(transaction => transaction.PaymentPageId == pageId);
                if (!transactions.Any())
                {
                    return NotFound("No transactions found for this payment page.");
                }
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets all transactions for the current authenticated user.
        /// </summary>
        /// <returns>A list of transactions associated with the current user.</returns>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllTransactions()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var transactions = await _transactionService.GetAllByUserAsync(userId);
                if (!transactions.Any())
                {
                    return NotFound("No transactions found.");
                }
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = "Failed to retrieve transactions.", Exception = ex.Message });
            }
        }
    }
}
