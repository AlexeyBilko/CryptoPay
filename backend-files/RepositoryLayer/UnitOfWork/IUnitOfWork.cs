using RepositoryLayer.Repository.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.UnitOfWork_
{
    public interface IUnitOfWork : IDisposable
    {
        ICurrencyRepository Currencies { get; }
        ISystemWalletRepository SystemWallets { get; }
        IUserWalletRepository UserWallets { get; }
        IPaymentPageRepository PaymentPages { get; }
        IPaymentPageTransactionRepository PaymentPageTransactions { get; }
        IAmountDetailsRepository AmountDetails { get; }
        IWithdrawalRepository Withdrawals { get; }
        IEarningsRepository Earnings { get; }
        Task<int> CompleteAsync();
    }

}
