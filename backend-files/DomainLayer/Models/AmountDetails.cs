using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Models
{
    // Details of financial amounts in both USD and cryptocurrency
    public class AmountDetails : BaseEntity<int>
    {
        [ForeignKey("Currency")]
        public int CurrencyId { get; set; }  // Links to the currency used in this transaction
        public decimal AmountUSD { get; set; }  // The amount in USD
        public decimal AmountCrypto { get; set; }  // The equivalent amount in cryptocurrency

        // One AmountDetails to One Currency
        public virtual Currency Currency { get; set; }
        public virtual ICollection<Withdrawal> Withdrawals { get; set; } = new List<Withdrawal>();
    }

}
