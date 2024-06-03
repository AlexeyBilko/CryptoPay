using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Models
{
    // User profile including authentication and personal preferences
    public class User : IdentityUser
    {
        public string? DisplayName { get; set; } // Display name chosen by the user
        public DateTime RegistrationDate { get; set; } // Date when the user registered
        public string Preferences { get; set; } // User-specific preferences in JSON format
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        public int RefreshTokenVersion { get; set; }
        public int TokenVersion { get; set; }

        // One User to Many PaymentPages, Transactions, Withdrawals, UserWallets; One User to One Earnings
        public virtual Earnings Earnings { get; set; }
        public virtual ICollection<PaymentPage> PaymentPages { get; set; } = new List<PaymentPage>();
        public virtual ICollection<UserWallet> UserWallets { get; set; } = new List<UserWallet>();
    }
}
