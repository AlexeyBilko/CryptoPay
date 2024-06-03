using Microsoft.AspNetCore.Mvc;
using ServiceLayer.DTOs;
using ServiceLayer.Service.Abstraction;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System;
using ServiceLayer.Service.Realization.IdentityServices;
using ServiceLayer.Service.Realization;

namespace CryptoPaymentGateway.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentPageController : ControllerBase
    {
        private readonly IPaymentPageService _paymentPageService;
        private readonly UserService _userService; // To manage users for association
        private readonly IAmountDetailsService _amountDetailsService;
        private readonly ICurrencyService _currencyService;

        public PaymentPageController(IPaymentPageService paymentPageService, UserService userService, IAmountDetailsService amountDetailsService, ICurrencyService currencyService)
        {
            _paymentPageService = paymentPageService;
            _userService = userService;
            _amountDetailsService = amountDetailsService;
            _currencyService = currencyService;
        }

        /// <summary>
        /// Creates a new payment page for the authenticated user.
        /// </summary>
        /// <param name="paymentPageDto">The DTO containing payment page details.</param>
        /// <returns>The created PaymentPageDTO if successful.</returns>
        [HttpPost("create")]
        public async Task<IActionResult> CreatePaymentPage([FromBody] PaymentPageRequest paymentPageDto)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not found or unauthorized.");
                }

                var user = await _userService.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                var currency = await _currencyService.GetByCurrencyCode(paymentPageDto.CurrencyCode);
                if (currency == null)
                {
                    return BadRequest("Invalid currency code.");
                }

                AmountDetailsDTO amountDetails;

                if (paymentPageDto.IsDonation)
                {
                    amountDetails = new AmountDetailsDTO
                    {
                        AmountUSD = 0,
                        AmountCrypto = 0,
                        CurrencyId = currency.Id
                    };
                }
                else
                {
                    amountDetails = new AmountDetailsDTO
                    {
                        AmountUSD = paymentPageDto.AmountUSD,
                        AmountCrypto = paymentPageDto.AmountCrypto,
                        CurrencyId = currency.Id
                    };
                }


                var createdAmountDetails = await _amountDetailsService.AddAsync(amountDetails);

                var paymentPage = new PaymentPageDTO
                {
                    UserId = userId,
                    AmountDetailsId = createdAmountDetails.Id,
                    IsDonation = paymentPageDto.IsDonation,
                    SystemWalletId = currency.CurrencyCode.ToLower() == "btc" ? 1 : 2,
                    Title = paymentPageDto.Title,
                    Description = paymentPageDto.Description
                };

                var createdPaymentPage = await _paymentPageService.AddAsync(paymentPage);
                return Ok(createdPaymentPage);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = "Failed to create payment page.", Exception = ex.Message });
            }
        }

        public class PaymentPageRequest
        {
            public int PageId { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string CurrencyCode { get; set; }
            public bool IsDonation { get; set; }
            public decimal AmountCrypto { get; set; }
            public decimal AmountUSD { get; set; }
        }

        /// <summary>
        /// Retrieves a payment page by its ID.
        /// </summary>
        /// <param name="pageId">The ID of the payment page to retrieve.</param>
        /// <returns>The PaymentPageDTO if found, or an error message if not.</returns>
        [AllowAnonymous]
        [HttpGet("{pageId}")]
        public async Task<IActionResult> GetPaymentPageById(string pageId)
        {
            try
            {
                var paymentPage = await _paymentPageService.GetAsync(int.Parse(pageId));
                if (paymentPage == null)
                {
                    return NotFound("Payment page not found.");
                }
                return Ok(paymentPage);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Retrieves all payment pages for the authenticated user.
        /// </summary>
        /// <returns>A list of PaymentPageDTOs associated with the user.</returns>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllPaymentPages()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Current user's ID
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not found or unauthorized.");
                }
                var allPages = _paymentPageService.GetAllByUserAsync(userId);

                // Filter pages by the current user
                return Ok(allPages);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = "Failed to retrieve payment pages.", Exception = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all payment pages for a specified user ID.
        /// </summary>
        /// <param name="userId">The ID of the user whose pages are to be retrieved.</param>
        /// <returns>A list of PaymentPageDTOs associated with the user.</returns>
        [HttpGet("allbyuserid/{userId}")]
        public async Task<IActionResult> GetAllPaymentPagesByUserId(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not found or unauthorized.");
                }
                //var userPages = (await _paymentPageService.GetAllByUserAsync(userId)).ToList();
                var allPages = (await _paymentPageService.GetAllAsync()).ToList();
                var userPages = allPages.Where(page => page.UserId == userId).ToList();

                if (!userPages.Any())
                {
                    return NotFound("No payment pages found for this user.");
                }
                return Ok(userPages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates a specified payment page.
        /// </summary>
        /// <param name="dto">The DTO containing updated page details.</param>
        /// <returns>The updated PaymentPageDTO if successful.</returns>
        [HttpPut("update")]
        public async Task<IActionResult> UpdatePaymentPage([FromBody] PaymentPageRequest paymentPageDto)
        {
            try
            {
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User not found or unauthorized.");
                }

                var user = await _userService.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                var paymentPageToEdit = await _paymentPageService.GetAsync(paymentPageDto.PageId);

                paymentPageToEdit.Description = paymentPageDto.Description;
                paymentPageToEdit.Title = paymentPageDto.Title;

                var updatedPage = await _paymentPageService.UpdateAsync(paymentPageToEdit);
                return Ok(updatedPage);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = "Failed to update payment page.", Exception = ex.Message });
            }
        }

        /// <summary>
        /// Deletes a specified payment page by ID.
        /// </summary>
        /// <param name="id">The ID of the payment page to delete.</param>
        /// <returns>200 OK if deletion was successful.</returns>
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeletePaymentPage(int id)
        {
            try
            {
                var page = await _paymentPageService.GetAsync(id);
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Current user's ID

                if (page == null || page.UserId != userId)
                    return Forbid(); // Prevent deleting pages not belonging to this user

                var result = await _paymentPageService.DeleteById(id);
                if (result) return Ok(new { Success = true });
                return NotFound(); // If page wasn't found or deletion failed
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = "Failed to delete payment page.", Exception = ex.Message });
            }
        }

        public class ConvertCryptoToUSDRequest
        {
            public decimal CryptoAmount { get; set; }
            public string CurrencyCode { get; set; }
        }

        public class ConvertUSDToCryptoRequest
        {
            public decimal USDAmount { get; set; }
            public string CurrencyCode { get; set; }
        }

        [HttpPost("convertToUSD")]
        public async Task<IActionResult> ConvertToUSD([FromBody] ConvertCryptoToUSDRequest request)
        {
            try
            {
                var amountUSD = await _currencyService.ConvertCryptoToUSD(request.CryptoAmount, request.CurrencyCode);
                return Ok(new { amountUSD });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = "Failed to convert crypto to USD.", Exception = ex.Message });
            }
        }

        [HttpPost("convertToCrypto")]
        public async Task<IActionResult> ConvertToCrypto([FromBody] ConvertUSDToCryptoRequest request)
        {
            try
            {
                var amountCrypto = await _currencyService.ConvertUSDToCrypto(request.USDAmount, request.CurrencyCode);
                return Ok(new { amountCrypto });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = "Failed to convert USD to crypto.", Exception = ex.Message });
            }
        }
    }
}

