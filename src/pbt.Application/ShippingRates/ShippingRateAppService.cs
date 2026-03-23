using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using pbt.ApplicationUtils;
using pbt.Core;
using pbt.Entities;
using pbt.ShippingRates.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace pbt.ShippingRates
{
    public class ShippingRateAppService : AsyncCrudAppService<ShippingRate, ShippingRateDto, long, PagedResultRequestDto, ShippingRateDto, ShippingRateDto>, IShippingRateAppService
    {
        public ShippingRateAppService(IRepository<ShippingRate, long> repository)
            : base(repository)
        {
        }

        public async Task<List<ShippingRateDto>> GetByGroupIdAsync(long groupId)
        {
            var query = await Repository.GetAllAsync();
            query = query.Where(sr => sr.ShippingRateGroupId == groupId);

            var shippingRates = await query.Include(sr => sr.Tiers) // Include bảng RateTier
                .ToListAsync();
            foreach (var rate in shippingRates)
            {
                rate.Tiers = rate.Tiers.OrderBy(t => t.ProductTypeId).ToList();
            }
            return ObjectMapper.Map<List<ShippingRateDto>>(shippingRates);
        }

        public async Task<ShippingCostResult> CalculateShippingCostForFinanceAsync(CalculateShippingInputDto input)
        {
            try
            {
                var statusCode = new SqlParameter("@StatusCode", System.Data.SqlDbType.Int) { Direction = System.Data.ParameterDirection.Output };
                var message = new SqlParameter("@Message", System.Data.SqlDbType.NVarChar, -1) { Direction = System.Data.ParameterDirection.Output };
                var result = await ConnectDb.GetItemAsync<ShippingCostResult>("dbo.SP_Packages_CalculateShippingCost_ByAttributes",
                System.Data.CommandType.StoredProcedure,
                new[]
                {
                new  SqlParameter("@CustomerId", input.CustomerId),
                new  SqlParameter("@WarehouseCreateId", input.WarehouseCreateId),
                new  SqlParameter("@WarehouseDestinationId", input.WarehouseDestinationId),
                new  SqlParameter("@ShippingLineId", input.ShippingLineId),
                new  SqlParameter("@ProductGroupTypeId", input.ProductGroupTypeId),
                new  SqlParameter("@Weight", input.Weight),
                new  SqlParameter("@Length", input.Length),
                new  SqlParameter("@Width", input.Width),
                new  SqlParameter("@Height", input.Height),
                new  SqlParameter("@IsWoodenCrate", input.IsWoodenCrate),
                new  SqlParameter("@IsShockproof", input.IsShockproof),
                new  SqlParameter("@DomesticShippingFeeRMB", input.DomesticShippingFeeRMB),
                new  SqlParameter("@Price", input.Price),
                statusCode,
                message

                });

                if ((int)statusCode.Value == (int)ExecuteStatus.Success)
                {
                    return result;
                }
                else
                {
                    throw new UserFriendlyException(Convert.ToString(message.Value));
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error in CalculateShippingCostForFinanceAsync: " + ex.Message, ex);
                throw ex;
            }


        }

    }
}
