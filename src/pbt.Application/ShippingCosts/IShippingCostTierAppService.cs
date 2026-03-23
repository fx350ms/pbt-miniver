using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using pbt.ShippingCosts.Dto;

namespace pbt.ShippingCosts
{
    /// <summary>
    /// ShippingCostTierAppService
    /// </summary>
    public interface IShippingCostTierAppService : IAsyncCrudAppService<
        ShippingCostTierDto, // DTO để trả về
        long,                // Kiểu dữ liệu của khóa chính
        PagedShippingCostTierResultRequestDto, // DTO để phân trang
        ShippingCostTierDto, // DTO để tạo mới
        ShippingCostTierDto> // DTO để cập nhật
    {
        /// <summary>
        /// Update the tier list for a shipping rate
        /// </summary>
        /// <param name="shippingRateId"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task UpdateTierList(long shippingRateId, List<ShippingCostTierDto> data);
    }
}