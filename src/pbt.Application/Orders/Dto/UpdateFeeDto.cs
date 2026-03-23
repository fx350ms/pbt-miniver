namespace pbt.Orders.Dto;

public class UpdateFeeDto
{
    public int Id { get; set; }
    public decimal? BubbleWrapFee { get; set; }
    public decimal? DomesticShipping { get; set; }
}