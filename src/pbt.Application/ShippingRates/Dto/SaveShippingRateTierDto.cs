namespace pbt.ShippingRates.Dto
{
    public class SaveShippingRateTierDto
    {
        public int ProductTypeId { get; set; } // Product type ID
        public decimal? FromValue { get; set; } // Starting value
        public decimal? ToValue { get; set; } // Ending value
        public string Unit { get; set; } // Unit (e.g., KG, M3)
        public decimal PricePerUnit { get; set; } // Price per unit
    }
}