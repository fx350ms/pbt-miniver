using Abp.Application.Services.Dto;

namespace pbt.Orders.Dto
{
    public class CreateCustomerOrderDto : EntityDto<long>
    {
        public string OrderNumber { get; set; }

        /// <summary>
        /// Mã vận đơn
        /// </summary>
        public string WaybillNumber { get; set; }


        /// <summary>
        /// Mã khách hàng
        /// </summary>
        public long CustomerId { get; set; }

        /// <summary>
        /// Tên khách hàng
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Loại đơn hàng sở hữu:
        /// True: Đơn của khách
        /// False: Đơn của mình
        /// </summary>
        public bool IsCustomerOrder { get; set; }

        /// <summary>
        /// Loại đơn
        /// 1. Đơn thường
        /// 2. Ký gửi
        /// </summary>
        public int OrderType { get; set; }


        /// <summary>
        /// Line vận chuyển: Chính ngạch, tiểu ngạch
        /// </summary>
        public int ShippingLine { get; set; }

        /// <summary>
        /// Trạng thái đơn hàng
        /// </summary>
        public int OrderStatus { get; set; }

        /// <summary>
        /// Trạng thái vận chuyển
        /// </summary>
        public int ShippingStatus { get; set; }

        public decimal CostPrice { get; set; }

        public decimal SellingPrice { get; set; }

        /// <summary>
        /// Id Của nhân viên kinh doanh
        /// </summary>
        public long UserSaleId { get; set; }


        public bool UseInsurance { get; set; }
        public bool UseWoodenPackaging { get; set; }
        public bool UseShockproofPackaging { get; set; }
        public bool UseDomesticTransportation { get; set; }
        public decimal Insurance { get; set; }

        public int CNWarehouseId { get; set; }
        public int VNWarehouseId { get; set; }
        public int AddressId { get; set; }
    }
}
