using DomainLayer.Models;
using ServiceLayer.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Service.Abstraction
{
    public interface IPaymentPageTransactionService : IService<PaymentPageTransaction, PaymentPageTransactionDTO, int>
    {
        Task<List<PaymentPageTransactionDTO>> GetTransactionsForReportAsync(string userId, DateTime startDate, DateTime endDate);
        Task<IEnumerable<PaymentPageTransactionDTO>> GetAllByUserAsync(string userId);
    }
}
