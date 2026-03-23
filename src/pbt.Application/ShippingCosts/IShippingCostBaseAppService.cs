using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using pbt.ShippingCosts.Dto;

namespace pbt.ShippingCosts
{
    public interface IShippingCostBaseAppService : IAsyncCrudAppService<
        ShippingCostBaseDto, // DTO để trả về
        long,            // Kiểu dữ liệu của khóa chính
        PagedResultRequestDto, // DTO để phân trang
        ShippingCostBaseDto, // DTO để tạo mới
        ShippingCostBaseDto> // DTO để cập nhật
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        Task<List<ShippingCostBaseDto>> GetByGroupIdAsync(long groupId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteShippingCostBaseAsync(long id);

    }
}