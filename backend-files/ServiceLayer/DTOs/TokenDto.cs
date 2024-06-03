using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.DTOs
{
    public class TokenDto
    {
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
