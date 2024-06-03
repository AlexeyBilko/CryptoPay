using DomainLayer.Models;
using ServiceLayer.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Service.Abstraction
{
    public interface IPaymentPageService : IService<PaymentPage, PaymentPageDTO, int>
    {
        Task<IEnumerable<PaymentPageDTO>> GetAllByUserAsync(string userId);
    }
}
