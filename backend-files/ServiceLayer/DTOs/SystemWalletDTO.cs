using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.DTOs
{
    public class SystemWalletDTO
    {
        public int Id { get; set; }
        public string WalletNumber { get; set; }
        public decimal Balance { get; set; }
        public string Title { get; set; }
        public string EncryptedWalletCodePhrase { get; set; }
    }
}
