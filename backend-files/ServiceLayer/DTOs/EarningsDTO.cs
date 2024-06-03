using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.DTOs
{
    public class EarningsDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public decimal TotalEarnedUSD { get; set; }
        public decimal TotalEarnedBTC { get; set; }
        public decimal TotalEarnedETH { get; set; }
        public decimal CurrentBalanceUSD { get; set; }
        public decimal CurrentBalanceBTC { get; set; }
        public decimal CurrentBalanceETH { get; set; }

        public UserDTO User { get; set; }
    }

}
