using Abp.Application.Services.Dto;
using System;
using System.ComponentModel.DataAnnotations;

namespace pbt.Orders.Dto
{
    public class CreateMyOrderDto : EntityDto<long>
    {
        public string OrderNumber { get; set; }

        /// <summary>
        /// Mã vận đơn
        /// </summary>
        public string WaybillNumber { get; set; }

        /// <summary>
        /// Loại đơn
        /// 1. Đơn thường
        /// 2. Ký gửi
        /// </summary>
        public int OrderType { get; set; }


        /// <summary>
        /// Line vận chuyển: Chính ngạch, tiểu ngạch
        /// </summary>
        /// 
        [Range(1, int.MaxValue)]
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

        public long CustomerId { get; set; }
        public string CustomerName { get; set; }
        public bool IsCustomerOrder { get; set; }
        
    }
}
