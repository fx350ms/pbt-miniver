using System;

namespace pbt.Bags.Dto
{
    public class BagViewForPartnerDto
    {
        public DateTime? ExportDateCN { get; set; } // Ngày xuất kho TQ
        public DateTime? ExportDateVN { get; set; } // Ngày xuất kho TQ
        public string Receiver { get; set; } // Người nhận
        public string BagCode { get; set; } // Mã bao
        public decimal? Weight { get; set; } // Cân nặng (Kg)
        public decimal? Volume { get; set; } // Kích thước (M3)
        public decimal? Length { get; set; } // Chiều dài
        public decimal? Width { get; set; } // Chiều rộng
        public decimal? Height { get; set; } // Chiều cao
        public int? TotalPackages { get; set; } // Tổng số kiện
        public string ShippingPartnerName { get; set; } // Đối tác vận chuyển
        public string Characteristic { get; set; } // Đặc tính (Dung dịch, Đóng gỗ, Hàng giả, Khác)
        public DateTime? ImportDateHN { get; set; } // Ngày nhập kho VN
        public string Note { get; set; } // Ghi chú
    }
}