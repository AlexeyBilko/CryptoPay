using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.DTOs
{
    public class PaymentPageDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int AmountDetailsId { get; set; }
        public int SystemWalletId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsDonation { get; set; }

        public UserDTO User { get; set; }
        public AmountDetailsDTO AmountDetails { get; set; }
        public SystemWalletDTO SystemWallet { get; set; }
    }

}
