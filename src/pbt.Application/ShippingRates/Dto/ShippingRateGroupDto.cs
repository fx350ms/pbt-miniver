using Abp.Application.Services.Dto;

namespace pbt.ShippingRates.Dto
{
    public class ShippingRateGroupDto : FullAuditedEntityDto<long>
    {
        public string Name { get; set; } // Tên bảng giá
        public string Note { get; set; } // Ghi chú
        public bool IsActived { get; set; } // Trạng thái kích hoạt
        public bool IsDefaultForCustomer { get; set; } // Mặc định cho khách hàng
    }
}