using DomainLayer.Models;
using RepositoryLayer.UnitOfWork_;
using ServiceLayer.DTOs;
using ServiceLayer.Service.Abstraction;
using ServiceLayer.Service.Realization.IdentityServices;
using ServiceLayer.Service.Realization.Mapper_;

namespace ServiceLayer.Service.Realization
{
    public class PaymentPageService : IPaymentPageService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly MyMapper _mapper;

        public PaymentPageService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _mapper = new MyMapper(unitOfWork);
        }

        public async Task<PaymentPageDTO> AddAsync(PaymentPageDTO dto)
        {
            var entity = _mapper.FromDTOtoPaymentPage(dto);
            var result = await _unitOfWork.PaymentPages.CreateAsync(entity);
            await _unitOfWork.CompleteAsync();
            return _mapper.Map<PaymentPage, PaymentPageDTO>(result);
        }

        public async Task<bool> DeleteAsync(PaymentPageDTO dto)
        {
            var entity = _mapper.FromDTOtoPaymentPage(dto);
            var result = await _unitOfWork.PaymentPages.DeleteAsync(entity);
            await _unitOfWork.CompleteAsync();
            return result;
        }

        public async Task<bool> DeleteById(int id)
        {
            var result = await _unitOfWork.PaymentPages.DeleteByIdAsync(id);
            await _unitOfWork.CompleteAsync();
            return result;
        }

        public async Task<IEnumerable<PaymentPageDTO>> GetAllAsync()
        {
            var entities = await _unitOfWork.PaymentPages.GetAllAsync();
            var dtos = new List<PaymentPageDTO>();
            foreach(var e in entities)
            {
                var tmp = await _mapper.PaymentPageToDTO(e);
                dtos.Add(tmp);
            }
            return dtos;
        }


        public async Task<IEnumerable<PaymentPageDTO>> GetAllByUserAsync(string userId)
        {
            var entities = _unitOfWork.PaymentPages.Query().Where(p => p.UserId == userId).ToList();
            if (entities.Any())
            {
                var dtos = await Task.WhenAll(entities.Select(async e => await _mapper.PaymentPageToDTO(e)));
                return dtos;
            }
            return new List<PaymentPageDTO>();
        }

        public async Task<PaymentPageDTO> GetAsync(int id)
        {
            var entity = await _unitOfWork.PaymentPages.GetByIdAsync(id);
            if (entity == null) throw new ArgumentException("Payment Page not found");
            return await _mapper.PaymentPageToDTO(entity);
        }

        public async Task<PaymentPageDTO> UpdateAsync(PaymentPageDTO dto)
        {
            var entity = _mapper.FromDTOtoPaymentPage(dto);
            var result = await _unitOfWork.PaymentPages.UpdateAsync(entity);
            await _unitOfWork.CompleteAsync();
            return await _mapper.PaymentPageToDTO(result);
        }
    }
}
