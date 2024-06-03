using DomainLayer.Models;
using RepositoryLayer.ApplicationDb;
using RepositoryLayer.Repository.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Repository.Realization
{
    public class PaymentPageTransactionRepository : GenericRepository<PaymentPageTransaction, int>, IPaymentPageTransactionRepository
    {
        public PaymentPageTransactionRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
