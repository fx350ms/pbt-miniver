using System.ComponentModel.DataAnnotations;

namespace pbt.Orders.Dto;

public class UpdateDeliveryFeeDto
{
    public int Id { get; set; }
    [Range(0, double.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
    [Required(ErrorMessage = "Giá là bắt buộc")]
    public decimal? UnitPrice { get; set; }
    [Required(ErrorMessage = "Số lượng là bắt buộc")]
    public decimal? Weight { get; set; }
    [Required(ErrorMessage = "Tổng phí là bắt buộc")]
    public decimal? TotalFee { get; set; }
    [Required(ErrorMessage = "Cập nhật lý do là bắt buộc")]
    public string? WeightUpdateReason { get; set; }
}