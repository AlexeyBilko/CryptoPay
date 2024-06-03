using DomainLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Repository.Abstraction
{
    public interface IPaymentPageTransactionRepository : IRepository<PaymentPageTransaction, int>
    {
    }
}
