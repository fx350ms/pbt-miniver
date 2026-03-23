using Abp.Application.Services.Dto;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using pbt.Entities;

namespace pbt.Orders.Dto
{
    public class CreateUpdateOrderDto : EntityDto<long>
    {
        public string OrderNumber { get; set; }

        /// <summary>
        /// Mã vận đơn
        /// </summary>
        public string WaybillNumber { get; set; }


        /// <summary>
        /// Mã khách hàng
        /// </summary>
        public long? CustomerId { get; set; }

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

        /// <summary>
        /// Số tiền thanh toán
        /// </summary>
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
        [DefaultValue(0)]
        [JsonConverter(typeof(DecimalJsonConverter))]
        public decimal? PriceInsurance { get; set; }

        public int CNWarehouseId { get; set; }
        public int VNWarehouseId { get; set; }
        public int AddressId { get; set; }
        public decimal GoodsValue { get; set; } 
        public decimal DomesticShipping { get; set; }  // Ship nội địa
        
        public decimal BubbleWrapFee { get; set; }  // Phí quấn bọt khí
        public decimal WoodenPackagingFee { get; set; }  // Phí đóng gỗ
        public decimal AmountDue { get; set; }  // Cần thanh toán
        public decimal TotalCost { get; set; }  // Tổng chi phí
        public decimal ECommerceShipping { get; set; }  //  chi phí can nag
        [DefaultValue(0)]
        public decimal? InsuranceValue { get; set; }  //  chi phí can nag


        // [ADDED] Các trường còn thiếu
        public decimal OrderFee { get; set; } // [ADDED]
        public decimal Paid { get; set; } // [ADDED]
        public int? PackageCount { get; set; } // [ADDED]
        public int PaymentStatus { get; set; } // [ADDED]
        public string Note { get; set; } // [ADDED]
        public long? ParentId { get; set; } // [ADDED]

        // [ADDED] Các trường thời gian
        public DateTime? InTransitToChinaWarehouseTime { get; set; } // [ADDED]
        public DateTime? InTransitTime { get; set; } // [ADDED]
        public DateTime? InTransitToVietnamWarehouseTime { get; set; } // [ADDED]
        public DateTime? DeliveryTime { get; set; } // [ADDED]
        public DateTime? ComplaintTime { get; set; } // [ADDED]
        public DateTime? RefundTime { get; set; } // [ADDED]
        public DateTime? CancelledTime { get; set; } // [ADDED]
        public DateTime? OrderCompletedTime { get; set; } // [ADDED]

    }
}
