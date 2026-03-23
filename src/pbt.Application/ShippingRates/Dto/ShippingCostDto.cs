namespace pbt.ShippingRates.Dto
{

    public class ShippingCostDto
    {
        // CustomerId
        public long? CustomerId { get; set; }
        // CNWarehouseId
        public int? CNWarehouseId { get; set; }
        // VNWarehouseId
        public int? VNWarehouseId { get; set; }
        // ShippingLineId
        public int? ShippingLineId { get; set; }
        // CostPerKg
        public long? CostPerKg { get; set; }
        // ProductGroupTypeId
        public int? ProductGroupTypeId { get; set; }
        // Weight
        public decimal? Weight { get; set; }
        // dimention
        public decimal? Dimension { get; set; }
    }
}