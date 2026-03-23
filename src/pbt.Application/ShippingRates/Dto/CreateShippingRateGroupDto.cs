using Abp.Application.Services.Dto;

namespace pbt.ShippingRates.Dto
{
    public class CreateShippingRateGroupDto
    {
        public long Id { get; set; } // ID của bảng giá vận chuyển
        public string Name { get; set; } // Tên bảng giá
        public string Note { get; set; } // Ghi chú
        public bool IsActived { get; set; } // Trạng thái kích hoạt
        public bool IsDefaultForCustomer { get; set; } // Mặc định cho khách hàng
        public byte[] Data { get; set; } // File đính kèm
    }
}