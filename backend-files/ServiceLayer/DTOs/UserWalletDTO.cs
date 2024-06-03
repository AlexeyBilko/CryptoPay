using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.DTOs
{
    public class UserWalletDTO
    {
        public int Id { get; set; }
        public string WalletNumber { get; set; }
        public string UserId { get; set; }
        public UserDTO User { get; set; }
    }
}
