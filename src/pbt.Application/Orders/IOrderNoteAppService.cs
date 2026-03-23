using Abp.Application.Services;
using pbt.Orders.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace pbt.Orders
{
    public interface IOrderNoteAppService : IAsyncCrudAppService<
        OrderNoteDto, // DTO để trả về
        long,         // Khóa chính của entity
        PagedOrderNoteResultRequestDto, // Dùng cho phân trang và sắp xếp
        OrderNoteDto,       // DTO để tạo mới
        OrderNoteDto>       // DTO để cập nhật
    {
        public Task<List<OrderNoteDto>> GetAllByOrderIdAsync(long orderId);
    }
}                                   