using Abp.Application.Services.Dto;
using System;
using System.ComponentModel.DataAnnotations;

namespace pbt.ShippingPartners.Dto
{
    public class CreateUpdateShippingPartnerDto : EntityDto<int>
    {
        [Required]
        [StringLength(256)]
        public string Code { get; set; }
        [Required]
        [StringLength(256)]
        public string Name { get; set; }
        [StringLength(256)]
        public string Phone { get; set; }
        [StringLength(256)]
        public string Email { get; set; }
        [StringLength(256)]
        public string Address { get; set; }
        public int? ApplicableWarehouse { get; set; }
        public int Type { get; set; }
        public int? MaxAmount { get; set; }
        public int? MaxWeigth { get; set; }
        public bool Status { get; set; }
    }
}
