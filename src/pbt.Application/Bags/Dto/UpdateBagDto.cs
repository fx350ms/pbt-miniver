using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using JetBrains.Annotations;

namespace pbt.Bags.Dto
{
public class UpdateBagDto : EntityDto<int>
    {
        [Required(ErrorMessage = "Vui lòng chọn kho đích")]
        [Range(1, int.MaxValue, ErrorMessage = "Kho đích không hợp lệ")]
        public int? WarehouseDestinationId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn phương thức vận chuyển")]
        [Range(1, int.MaxValue, ErrorMessage = "Phương thức vận chuyển không hợp lệ")] 
        public int? ShippingType { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Khách hàng không hợp lệ")]
        public int? CustomerId { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn loại bao")]
        [Range(1, int.MaxValue, ErrorMessage = "Loại bao không hợp lệ")]
        public int? BagType { get; set; }
        public int ShippingPartnerId { get; set; }
        //public decimal? Weight { get; set; }
        public decimal? Length { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        
        public bool IsSolution { get; set; } // Dung dịch
        public bool IsWoodSealing { get; set; } // Đóng gỗ
        public bool IsFakeGoods { get; set; } // Hàng giả
        public bool IsOtherFeature { get; set; } // Khác
        [CanBeNull] public string otherReason { get; set; }
        public string Receiver { get; set; }
        public string Note { get; set; }
        // decimal? WeightPackage { get; set; }
        //public int? BagSize { get; set; }
        //public decimal? WeightPackages { get; set; }
    }
}