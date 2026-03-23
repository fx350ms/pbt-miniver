using Abp.Application.Services;
using Abp.Application.Services.Dto;
using pbt.Application.WarehouseTransfers.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pbt.Application.WarehouseTransfers
{
    public interface IWarehouseTransferDetailAppService : IAsyncCrudAppService<
        WarehouseTransferDetailDto, // DTO chính
        long,                       // Kiểu dữ liệu của khóa chính
        PagedAndSortedResultRequestDto, // DTO cho phân trang và sắp xếp
        WarehouseTransferDetailDto, // DTO cho tạo mới
        WarehouseTransferDetailDto  // DTO cho cập nhật
    >
    {
        Task<List<WarehouseTransferDetailDto>> GetDetailsByWarehouseTransferIdAsync(int warehouseTransferId);
    }
}