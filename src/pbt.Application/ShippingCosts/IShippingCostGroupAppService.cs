using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Microsoft.AspNetCore.Mvc;
using pbt.ShippingCosts.Dto;
using pbt.ShippingRates.Dto;

namespace pbt.ShippingCosts
{
    public interface IShippingCostGroupAppService : IAsyncCrudAppService<
        ShippingCostGroupDto, // DTO để trả về
        long,                 // Kiểu dữ liệu của khóa chính
        PagedResultRequestDto, // DTO để phân trang
        ShippingCostGroupDto, // DTO để tạo mới
        ShippingCostGroupDto> // DTO để cập nhật
    {
        Task<IActionResult> saveShippingCosts(SaveShippingCostDto input);
    }
}