using Abp.Application.Services.Dto;
using System.ComponentModel.DataAnnotations;

namespace pbt.Warehouses.Dto
{
    public class CreateUpdateWarehouseDto : EntityDto<int>
    {
        [Required]
        [StringLength(256)]
        public string Code { get; set; }
        [Required]
        [StringLength(256)]
        public string Name { get; set; }
        [StringLength(256)]
        public string? Phone { get; set; }
        [StringLength(256)]
        public string Country { get; set; }
        [StringLength(256)]
        public string City { get; set; }
        [StringLength(256)]
        public string District { get; set; }
        [StringLength(256)]
        public string? Ward { get; set; }
        public string Address { get; set; }
        public string FullAddress { get; set; }
        public bool Status { get; set; }

        public int CountryId { get; set; }
    }
}
