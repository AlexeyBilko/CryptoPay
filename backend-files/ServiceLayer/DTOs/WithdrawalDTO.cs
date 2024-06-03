using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.DTOs
{
    public class WithdrawalDTO
    {
        public int Id { get; set; }
        public int SystemWalletId { get; set; }
        public int UserWalletId { get; set; }
        public int AmountDetailsId { get; set; }
        public string Status { get; set; }
        public string TransactionId { get; set; }
        public DateTime RequestedDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        public SystemWalletDTO SystemWallet { get; set; }
        public UserWalletDTO UserWallet { get; set; }
        public AmountDetailsDTO AmountDetails { get; set; }
    }
}
