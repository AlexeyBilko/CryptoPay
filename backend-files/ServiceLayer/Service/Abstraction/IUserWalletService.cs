﻿using DomainLayer.Models;
using ServiceLayer.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Service.Abstraction
{
    public interface IUserWalletService : IService<UserWallet, UserWalletDTO, int>
    {
        Task<UserWalletDTO> GetByWalletAddress(string walletNumber);
    }
}
