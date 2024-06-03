using RepositoryLayer.ApplicationDb;
using RepositoryLayer.Repository.Abstraction;
using RepositoryLayer.Repository.Realization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.UnitOfWork_
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public ICurrencyRepository Currencies { get; private set; }
        public ISystemWalletRepository SystemWallets { get; private set; }
        public IUserWalletRepository UserWallets { get; private set; }
        public IPaymentPageRepository PaymentPages { get; private set; }
        public IPaymentPageTransactionRepository PaymentPageTransactions { get; private set; }
        public IAmountDetailsRepository AmountDetails { get; private set; }
        public IWithdrawalRepository Withdrawals { get; private set; }
        public IEarningsRepository Earnings { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Currencies = new CurrencyRepository(_context);
            SystemWallets = new SystemWalletRepository(_context);
            UserWallets = new UserWalletRepository(_context);
            PaymentPages = new PaymentPageRepository(_context);
            PaymentPageTransactions = new PaymentPageTransactionRepository(_context);
            AmountDetails = new AmountDetailsRepository(_context);
            Withdrawals = new WithdrawalRepository(_context);
            Earnings = new EarningsRepository(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

}
