using System.Collections.Generic;
using System.Threading.Tasks;
using pbt.Commons.Dto;
using pbt.Entities;
using pbt.WoodenPackings.Dto;

namespace pbt.WoodenPackings;

public interface IWoodenPackingService
{
    /// <summary>
    /// Lấy tất cả đóng bao chung để chọn
    /// </summary>
    /// <param name="q"></param>
    /// <returns></returns>
    Task<List<OptionItemDto>> GetAllForSelectAsync(string q = "");

    /// <summary>
    /// Tạo mới đóng bao chung
    /// </summary>
    /// <param name="package"></param>
    /// <param name="customerCode"></param>
    /// <param name="orderCode"></param>
    /// <returns></returns>
    Task<Package> CreateWoodenPacking(CreateWoodenPackingDto createWoodenPackingDto, string customerCode, string orderCode);

    /// <summary>
    /// 
    /// Cập nhật đóng gỗ chung khi cập nhật kiện hàng   
    /// </summary>
    /// <param name="package"></param>
    /// <param name="woodenCrateType"></param>
    /// <param name="customerCode"></param>
    /// <param name="orderCode"></param>
    /// <returns></returns>
    Task<Package> UpdateWoodenPacking(Package package, long? sharedCrateSelectId, string customerCode, string orderCode);
}