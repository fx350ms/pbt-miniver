using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Entities
{

    public class Warehouse :  FullAuditedEntity<int>
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
        public string Ward { get; set; }
        public string Address { get; set; }
        public string FullAddress { get; set; }
        public bool Status { get; set; }

        [StringLength(20)]
        public string Receiver { get; set; }
        [StringLength(20)]
        public string PostCode { get; set; }

        /// <summary>
        /// 1: TQ
        /// 2: VN
        /// </summary>
        public int CountryId { get; set; }

    }

}
