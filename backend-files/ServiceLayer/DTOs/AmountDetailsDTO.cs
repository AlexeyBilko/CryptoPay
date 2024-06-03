using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.DTOs
{
    public class AmountDetailsDTO
    {
        public int Id { get; set; }
        public int CurrencyId { get; set; }
        public decimal AmountUSD { get; set; }
        public decimal AmountCrypto { get; set; }

        public CurrencyDTO Currency { get; set; }
    }

}
