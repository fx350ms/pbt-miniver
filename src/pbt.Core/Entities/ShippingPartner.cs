using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Domain.Entities;

namespace pbt.Entities
{

    public class ShippingPartner :  Entity<int>
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
