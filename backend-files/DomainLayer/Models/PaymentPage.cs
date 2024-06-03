using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Models
{
    public class PaymentPage : BaseEntity<int>
    {
        [ForeignKey("User")]
        public string UserId { get; set; } // Foreign key to the user who owns this page
        [ForeignKey("AmountDetails")]
        public int AmountDetailsId { get; set; } // Financial details linked to this page
        [ForeignKey("SystemWallet")]
        public int SystemWalletId { get; set; } // Identifies the system wallet receiving the funds
        public string Title { get; set; }
        public string Description { get; set; } // Payment Page short Description
        public bool IsDonation { get; set; }

        // One PaymentPage to One User, One AmountDetails
        public virtual User User { get; set; }
        public virtual AmountDetails AmountDetails { get; set; }
        public virtual SystemWallet SystemWallet { get; set; }
        public virtual ICollection<PaymentPageTransaction> PaymentPageTransactions { get; set; } = new List<PaymentPageTransaction>();
    }

}
