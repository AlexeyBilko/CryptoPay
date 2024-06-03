using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Models
{
    // Financial performance of a user over time
    public class Earnings : BaseEntity<int>
    {
        [ForeignKey("User")]
        public string UserId { get; set; } // Foreign key linking to the User
        public decimal TotalEarnedUSD { get; set; } // Total earnings in USD
        public decimal TotalEarnedBTC { get; set; } // Total earnings in BTC
        public decimal TotalEarnedETH { get; set; } // Total earnings in ETH
        public decimal CurrentBalanceUSD { get; set; } // Current balance in USD
        public decimal CurrentBalanceBTC { get; set; } // Current balance in BTC
        public decimal CurrentBalanceETH { get; set; } // Current balance in ETH

        // One Earnings to One User
        public virtual User User { get; set; }
    }

}
