using Abp.Domain.Entities;
using System;

namespace pbt.Entities
{
    public class WoodenPacking : Entity<long>
    {
        public string WoodenPackingCode { get; set; }
        public decimal WeightTotal { get; set; }
        public decimal VolumeTotal { get; set; }
        public decimal CostTotal { get; set; }
    }
}
