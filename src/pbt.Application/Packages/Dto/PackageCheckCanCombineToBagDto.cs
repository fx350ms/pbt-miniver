using System;

namespace pbt.Packages.Dto
{
    public class PackageCheckCanCombineToBagDto
    {
        public int Id { get; set; }

        public string PackageNumber { get; set; } = string.Empty;

        public bool IsDiff_CreateWarehouse { get; set; }

        public bool IsDiff_DestinationWarehouse { get; set; }

        public bool IsDiff_ShippingLine { get; set; }

        public bool IsDiff_Status { get; set; }
    }
}
