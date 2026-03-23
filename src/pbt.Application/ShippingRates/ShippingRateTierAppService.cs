using Abp.Application.Services.Dto;
using Abp.Application.Services;
using Abp.Domain.Repositories;
using pbt.Entities;
using pbt.ShippingRates.Dto;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;


namespace pbt.ShippingRates
{
    public class ShippingRateTierAppService : AsyncCrudAppService<ShippingRateTier, ShippingRateTierDto, long, PagedShippingRateTierResultRequestDto, ShippingRateTierDto, ShippingRateTierDto>, IShippingRateTierAppService
    {
        public ShippingRateTierAppService(IRepository<ShippingRateTier, long> repository)
            : base(repository)
        {
        }

        public override Task<PagedResultDto<ShippingRateTierDto>> GetAllAsync(PagedShippingRateTierResultRequestDto input)
        {
            return base.GetAllAsync(input);
        }


        public async Task UpdateTierList(long shippingRateId, List<ShippingRateTierDto> data) 
        {
            // Get the existing tiers for the shipping rate
            var existingTiers = await Repository.GetAllListAsync(x => x.ShippingRateId == shippingRateId);

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
                    var newTier = ObjectMapper.Map<ShippingRateTier>(tier);
                    newTier.ShippingRateId = shippingRateId;
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
