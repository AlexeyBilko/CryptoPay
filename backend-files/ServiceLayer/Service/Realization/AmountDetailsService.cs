using DomainLayer.Models;
using RepositoryLayer.UnitOfWork_;
using ServiceLayer.DTOs;
using ServiceLayer.Service.Abstraction;
using ServiceLayer.Service.Realization.IdentityServices;
using ServiceLayer.Service.Realization.Mapper_;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Service.Realization
{
    public class AmountDetailsService : IAmountDetailsService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly MyMapper mapper;

        public AmountDetailsService(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
            mapper = new MyMapper(unitOfWork);
        }

        public async Task<AmountDetailsDTO> AddAsync(AmountDetailsDTO entity)
        {
            var mappedEntity = await mapper.MapAsync<AmountDetailsDTO, AmountDetails>(entity);
            var result = await unitOfWork.AmountDetails.CreateAsync(mappedEntity);
            await unitOfWork.CompleteAsync();

            return await mapper.MapAsync<AmountDetails, AmountDetailsDTO>(result);
        }

        public async Task<bool> DeleteAsync(AmountDetailsDTO entity)
        {
            var mappedEntity = await mapper.MapAsync<AmountDetailsDTO, AmountDetails>(entity);

            var result = await unitOfWork.AmountDetails.DeleteAsync(mappedEntity);
            await unitOfWork.CompleteAsync();

            return result;
        }

        public async Task<bool> DeleteById(int id)
        {
            var result = await unitOfWork.AmountDetails.DeleteByIdAsync(id);
            await unitOfWork.CompleteAsync();

            return result;
        }

        public async Task<AmountDetailsDTO> UpdateAsync(AmountDetailsDTO entity)
        {
            var mappedEntity = await mapper.MapAsync<AmountDetailsDTO, AmountDetails>(entity);
            var result = await unitOfWork.AmountDetails.UpdateAsync(mappedEntity);
            await unitOfWork.CompleteAsync();

            return await mapper.MapAsync<AmountDetails, AmountDetailsDTO>(result);
        }

        public async Task<IEnumerable<AmountDetailsDTO>> GetAllAsync()
        {
            return await (await unitOfWork.AmountDetails.GetAllAsync())
                .Select(async x => await mapper.MapAsync<AmountDetails, AmountDetailsDTO>(x)).WhenAll();
        }

        public async Task<AmountDetailsDTO> GetAsync(int id)
        {
            var answer = await unitOfWork.AmountDetails.GetByIdAsync(id);
            if (answer != null)
            {
                return await mapper.MapAsync<AmountDetails, AmountDetailsDTO>(answer);
            }
            throw new ArgumentException("Amount Details not found");
        }
    }
}
