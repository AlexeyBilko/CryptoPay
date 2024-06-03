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
    public class EarningsRepository : GenericRepository<Earnings, int>, IEarningsRepository
    {
        public EarningsRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
