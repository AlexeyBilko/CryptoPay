using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Models
{
    // Record of each transaction processed through the system
    public class PaymentPageTransaction : BaseEntity<int>
    {
        [ForeignKey("PaymentPage")]
        public int PaymentPageId { get; set; } // Links to the corresponding payment page
        public string TransactionHash { get; set; } // Blockchain hash of the transaction
        public string SenderWalletAddress { get; set; } // Sending wallet address
        public string Status { get; set; } // Status of the transaction, e.g., pending, completed
        public DateTime CreatedAt { get; set; } // Timestamp of the transaction

        // Blockchain specific metadata
        public int BlockNumber { get; set; } // The block number in which the transaction was recorded on the blockchain
        public DateTime BlockTimestamp { get; set; } // The timestamp provided by the blockchain for when the block was mined
        public string TransactionIndex { get; set; } // Position of the transaction within the block
        public decimal? GasPrice { get; set; } // Gas price used for Ethereum transactions (ETH specific)
        public decimal? GasUsed { get; set; } // Gas used for the transaction (ETH specific)
        public string InputData { get; set; } // Additional input data sent with the transaction (useful for contract interactions)
        public decimal TransactionFee { get; set; } // Transaction fee in the cryptocurrency of the transaction
        public decimal ActualAmountCrypto { get; set; } // Transaction amount in the cryptocurrency specified on the payment page
        public string PayerEmail { get; set; }


        // One Transaction to One PaymentPage, One AmountDetails
        public virtual PaymentPage PaymentPage { get; set; }
    }
}
