using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using JetBrains.Annotations;
using pbt.Packages.Dto;
using pbt.Warehouses.Dto;

namespace pbt.Bags.Dto
{
public class CreateUpdateBagDto : EntityDto<int>
    {
        [StringLength(50, ErrorMessage = "Mã bao không được vượt quá 50 ký tự")]
        public string BagCode { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn loại bao")]
        [Range(1, int.MaxValue, ErrorMessage = "Loại bao không hợp lệ")]
        public int? BagType { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Khách hàng không hợp lệ")]
        public int? CustomerId { get; set; }
        public bool IsSolution { get; set; } // Dung dịch
        public bool IsWoodSealing { get; set; } // Đóng gỗ  
        public bool IsFakeGoods { get; set; } // Hàng giả
        public bool IsOtherFeature { get; set; } // Khác
        [Required(ErrorMessage = "Vui lòng chọn kho tạo")]
        [Range(1, int.MaxValue, ErrorMessage = "Kho tạo không hợp lệ")]
        public int? WarehouseCreateId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn kho đích")]
        [Range(1, int.MaxValue, ErrorMessage = "Kho đích không hợp lệ")]
        public int? WarehouseDestinationId { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập người nhận")]
        [StringLength(100, ErrorMessage = "Người nhận không được vượt quá 100 ký tự")]
        public string Receiver { get; set; }
        [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
        [CanBeNull] public string Note { get; set; }
        public bool IsClosed { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn đối tác vận chuyển")]
        [Range(1, int.MaxValue, ErrorMessage = "Đối tác vận chuyển không hợp lệ")]
        public int? ShippingPartnerId { get; set; }
        [StringLength(100, ErrorMessage = "Tên đối tác vận chuyển không được vượt quá 100 ký tự")]
        public string? ShippingPartnerName { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn phương thức vận chuyển")]
        [Range(1, int.MaxValue, ErrorMessage = "Phương thức vận chuyển không hợp lệ")] 
        public int? ShippingType { get; set; }
        [Range(0, int.MaxValue, ErrorMessage = "Trạng thái kho không hợp lệ")]
        public int? WarehouseStatus { get; set; }
        [Range(0, 9999999.99, ErrorMessage = "Khối lượng không hợp lệ")]
        public decimal? Weight { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? WeightPackage { get; set; }
        public int? BagSize { get; set; }
        public decimal? WeightPackages { get; set; }
        
        [CanBeNull] public virtual WarehouseDto WarehouseCreate { get; set; }
        [CanBeNull] public virtual WarehouseDto WarehouseDestination { get; set; } 
        public virtual List<PackageDto> Packages { get; set; }
        public string CreationTimeFormat { get; set; }
        public decimal? Volume { get; set; }
        [CanBeNull] public string otherReason { get; set; }
        public List<int> selectedPackages { get; set; }

    }
}
