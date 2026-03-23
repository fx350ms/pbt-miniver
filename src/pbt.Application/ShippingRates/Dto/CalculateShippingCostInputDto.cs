using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.ShippingRates.Dto
{
    public class CalculateShippingInputDto
    {
        // Thông tin chung
        /// <summary>
        /// Mã khách hàng (Tương đương @CustomerId)
        /// </summary>
        public long CustomerId { get; set; }

        /// <summary>
        /// ID Kho hàng xuất phát (Tương đương @WarehouseCreateId)
        /// </summary>
        public long WarehouseCreateId { get; set; }

        /// <summary>
        /// ID Kho hàng đích (Tương đương @WarehouseDestinationId)
        /// </summary>
        public long WarehouseDestinationId { get; set; }

        /// <summary>
        /// ID Tuyến vận chuyển (Tương đương @ShippingLineId)
        /// </summary>
        public int ShippingLineId { get; set; }

        /// <summary>
        /// Loại nhóm sản phẩm (Tương đương @ProductGroupTypeId)
        /// </summary>
        public int ProductGroupTypeId { get; set; }


        // Thông số Hàng hóa (Cân nặng & Kích thước)
        /// <summary>
        /// Cân nặng (Tương đương @Weight)
        /// </summary>
        public decimal Weight { get; set; }

        /// <summary>
        /// Chiều dài (Tương đương @Length)
        /// </summary>
        public decimal Length { get; set; }

        /// <summary>
        /// Chiều rộng (Tương đương @Width)
        /// </summary>
        public decimal Width { get; set; }

        /// <summary>
        /// Chiều cao (Tương đương @Height)
        /// </summary>
        public decimal Height { get; set; }


        // Dịch vụ Gia tăng & Phí đã có
        /// <summary>
        /// Có sử dụng Đóng gỗ không? (Tương đương @IsWoodenCrate - BIT)
        /// </summary>
        public bool IsWoodenCrate { get; set; }

        /// <summary>
        /// Có sử dụng Chống sốc (Quấn bọt khí) không? (Tương đương @IsShockproof - BIT)
        /// </summary>
        public bool IsShockproof { get; set; }

        /// <summary>
        /// Phí vận chuyển nội địa (tiền RMB) đã có (Tương đương @DomesticShippingFeeRMB)
        /// </summary>
        public decimal DomesticShippingFeeRMB { get; set; }

        public decimal Price { get; set; }  
    }
}
