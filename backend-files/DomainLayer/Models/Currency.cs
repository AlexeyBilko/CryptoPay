using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Models
{
    // Currency information used across various financial transactions
    public class Currency : BaseEntity<int>
    {
        public string CurrencyCode { get; set; } // ISO currency code, e.g., BTC, ETH
        public string CurrencyName { get; set; } // Descriptive name of the currency
        public string Network { get; set; } // Blockchain network, e.g., Ethereum, Bitcoin

        public virtual ICollection<AmountDetails> AmountDetailss { get; set; } = new List<AmountDetails>();
    }
}
