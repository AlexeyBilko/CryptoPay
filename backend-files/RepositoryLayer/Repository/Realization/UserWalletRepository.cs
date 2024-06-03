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
    public class UserWalletRepository : GenericRepository<UserWallet, int>, IUserWalletRepository
    {
        public UserWalletRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
