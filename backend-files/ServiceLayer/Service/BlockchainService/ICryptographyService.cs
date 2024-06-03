using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Service.BlockchainService
{
    public interface ICryptographyService
    {
        string Encrypt(string input);
        string Decrypt(string cipherText);
    }
}
