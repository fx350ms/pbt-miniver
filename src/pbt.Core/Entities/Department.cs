using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Entities
{

    public class Department :  FullAuditedEntity<int>
    {
        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        [StringLength(256)]
        public string Description { get; set; }

        public int? ParentId { get; set; } // Phòng ban cha (nếu có)

        public Department Parent { get; set; } // Quan hệ với phòng ban cha
    }

}
