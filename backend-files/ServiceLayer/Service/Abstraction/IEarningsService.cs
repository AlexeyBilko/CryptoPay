using DomainLayer.Models;
using ServiceLayer.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Service.Abstraction
{
    public interface IEarningsService : IService<Earnings, EarningsDTO, int>
    {
        Task<EarningsDTO> GetEarningsForReportAsync(string userId, DateTime startDate, DateTime endDate);
        Task<bool> UpdateEarningsUSDForUser(string userId);
        Task<EarningsDTO> GetEarningsByUserId(string userId);
    }
}
