using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Microsoft.EntityFrameworkCore;
using pbt.Entities;
using pbt.ShippingCosts.Dto;
using pbt.ShippingRates.Dto;

namespace pbt.ShippingCosts
{
    /// <summary>
    /// ShippingCostBaseAppService
    /// </summary>
    public class ShippingCostBaseAppService : AsyncCrudAppService<
            ShippingCostBase, // Entity
            Dto.ShippingCostBaseDto, // DTO để trả về
            long, // Kiểu dữ liệu của khóa chính
            PagedResultRequestDto, // DTO để phân trang
            Dto.ShippingCostBaseDto, // DTO để tạo mới
            Dto.ShippingCostBaseDto>, // DTO để cập nhật
            IShippingCostBaseAppService
    {
        IRepository<ShippingCostTier, long> _shippingCostTierRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="shippingCostTierRepository"></param>
        public ShippingCostBaseAppService(IRepository<ShippingCostBase, long> repository, IRepository<ShippingCostTier, long> shippingCostTierRepository, IUnitOfWorkManager unitOfWorkManager)
            : base(repository)
        {
            _shippingCostTierRepository = shippingCostTierRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public async Task<List<ShippingCostBaseDto>> GetByGroupIdAsync(long groupId)
        {
            var query = await Repository.GetAllAsync();
            query = query.Where(sr => sr.ShippingCostGroupId == groupId);

            var shippingRates = await query.Include(sr => sr.Tiers) // Include bảng RateTier
                .ToListAsync();
            foreach (var rate in shippingRates)
            {
                rate.Tiers = rate.Tiers.OrderBy(t => t.ProductTypeId).ToList();
            }

            return ObjectMapper.Map<List<ShippingCostBaseDto>>(shippingRates);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        [UnitOfWork]
        public virtual async Task DeleteShippingCostBaseAsync(long id)
        {
            using var uow = _unitOfWorkManager.Begin();
            try
            {
                var tiers = await _shippingCostTierRepository.GetAllListAsync(t => t.ShippingCostBaseId == id);
                foreach (var tier in tiers)
                {
                    await _shippingCostTierRepository.DeleteAsync(tier);
                }

                var entity = await Repository.FirstOrDefaultAsync(x => x.Id == id);
                if (entity != null)
                {
                    await Repository.DeleteAsync(entity);
                }
                await uow.CompleteAsync();
            }
            catch
            {
                // No need to call RollbackAsync(); disposing uow will rollback automatically
                throw;
            }
        }
    }
}