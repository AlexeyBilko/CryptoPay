using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Models
{
    // Represents a wallet owned by a user
    public class UserWallet : BaseEntity<int>
    {
        public string WalletNumber { get; set; } // Unique identifier of the wallet
        [ForeignKey("User")]
        public string UserId { get; set; } // Foreign key to the user

        public virtual User User { get; set; }
        public virtual ICollection<Withdrawal> Withdrawals { get; set; } = new List<Withdrawal>();
    }

}
