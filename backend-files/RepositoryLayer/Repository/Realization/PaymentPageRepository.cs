using DomainLayer.Models;
using RepositoryLayer.ApplicationDb;
using RepositoryLayer.Repository.Abstraction;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Repository.Realization
{
    public class PaymentPageRepository : GenericRepository<PaymentPage, int>, IPaymentPageRepository
    {
        public PaymentPageRepository(ApplicationDbContext context) : base(context)
        {
        }

        //public override async Task<IEnumerable<PaymentPage>> GetAllAsync()
        //{
        //    return await context.PaymentPages
        //                        .Include(p => p.AmountDetails)
        //                        .ThenInclude(ad => ad.Currency)
        //                        .Include(p => p.User)
        //                        .ToListAsync();
        //}
    }
}
