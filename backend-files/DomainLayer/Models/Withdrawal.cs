using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Models
{
    // Details about withdrawals initiated by users
    public class Withdrawal : BaseEntity<int>
    {
        [ForeignKey("SystemWallet")]
        public int SystemWalletId { get; set; } // System wallet from which the withdrawal is made
        [ForeignKey("UserWallet")]
        public int UserWalletId { get; set; } // User's wallet to which the withdrawal is sent
        [ForeignKey("AmountDetails")]
        public int AmountDetailsId { get; set; } // Financial details of the withdrawal
        public string Status { get; set; } // Current status of the withdrawal, e.g., pending, completed
        public string TransactionId { get; set; }
        public DateTime RequestedDate { get; set; }  // Date when the withdrawal was requested
        public DateTime? CompletedDate { get; set; }  // Date when the withdrawal was completed, if applicable

        public virtual SystemWallet SystemWallet { get; set; }
        public virtual UserWallet UserWallet { get; set; }
        public virtual AmountDetails AmountDetails { get; set; }
    }

}
