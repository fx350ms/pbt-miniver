using Abp.Application.Services.Dto;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using pbt.Entities;
using pbt.ShippingRates.Dto;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using pbt.ShippingCosts.Dto;

namespace pbt.ShippingCosts
{
    public class ShippingCostTierAppService : AsyncCrudAppService<ShippingCostTier, ShippingCostTierDto, long, PagedShippingCostTierResultRequestDto, ShippingCostTierDto, ShippingCostTierDto>, IShippingCostTierAppService
    {
        public ShippingCostTierAppService(IRepository<ShippingCostTier, long> repository)
            : base(repository)
        {
            
        }
        
        public override Task<PagedResultDto<ShippingCostTierDto>> GetAllAsync(PagedShippingCostTierResultRequestDto input)
        {
            return base.GetAllAsync(input);
        }

        /// <summary>
        /// Update the tier list for a shipping rate
        /// </summary>
        /// <param name="shippingRateId"></param>
        /// <param name="data"></param>
        public async Task UpdateTierList(long shippingRateId, List<ShippingCostTierDto> data) 
        {
            // Get the existing tiers for the shipping rate
            var existingTiers = await Repository.GetAllListAsync(x => x.ShippingCostBaseId == shippingRateId);

            // Remove the existing tiers that are not in the new list
            foreach (var existingTier in existingTiers)
            {
                var tierToRemove = data.FirstOrDefault(x => x.Id == existingTier.Id);
                if (tierToRemove == null)
                {
                    await Repository.DeleteAsync(existingTier);
                }
            }

            // Add or update the tiers in the new list
            foreach (var tier in data)
            {
                if (tier.Id == 0) // New tier
                {
                    var newTier = ObjectMapper.Map<ShippingCostTier>(tier);
                    newTier.ShippingCostBaseId = shippingRateId;
                    await Repository.InsertAsync(newTier);
                }
                else // Existing tier
                {
                    var existingTier = await Repository.GetAsync(tier.Id);
                    ObjectMapper.Map(tier, existingTier);
                    await Repository.UpdateAsync(existingTier);
                }
            }
        }
    }
}
