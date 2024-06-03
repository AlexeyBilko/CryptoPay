using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.DTOs
{
    public class PaymentPageTransactionDTO
    {
        public int Id { get; set; }
        public int PaymentPageId { get; set; }
        public string TransactionHash { get; set; }
        public string SenderWalletAddress { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int BlockNumber { get; set; }
        public DateTime BlockTimestamp { get; set; }
        public string TransactionIndex { get; set; }
        public decimal? GasPrice { get; set; }
        public decimal? GasUsed { get; set; }
        public string InputData { get; set; }
        public decimal TransactionFee { get; set; }
        public decimal ActualAmountCrypto { get; set; }
        public string PayerEmail { get; set; }

        public PaymentPageDTO PaymentPage { get; set; }
    }
}
