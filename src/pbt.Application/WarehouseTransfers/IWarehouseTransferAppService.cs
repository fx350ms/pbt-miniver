using Abp.Application.Services;
using pbt.WarehouseTransfers.Dto;

namespace pbt.WarehouseTransfers
{
    public interface IWarehouseTransferAppService : IAsyncCrudAppService<
        WarehouseTransferDto, // DTO để trả về
        int,                  // Kiểu dữ liệu của khóa chính
        PagedWarehouseTransferResultRequestDto, // DTO để phân trang
        WarehouseTransferDto,       // DTO để tạo mới
        WarehouseTransferDto>       // DTO để cập nhật
    {
    }
}