using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Models
{
    // Represents a wallet associated with the system, not user-specific (holds funds)
    public class SystemWallet : BaseEntity<int>
    {
        public string WalletNumber { get; set; } // Unique identifier of the wallet
        public decimal Balance { get; set; } // Current balance in the wallet in USD
        public string EncryptedWalletCodePhrase { get; set; }
        public string Title { get; set; }

        public virtual ICollection<PaymentPage> PaymentPages { get; set; } = new List<PaymentPage>();
        public virtual ICollection<Withdrawal> Withdrawals { get; set; } = new List<Withdrawal>();
    }
}
