using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Entities
{
    public class IdentityCode : Entity<long>
    {
        public long Date { get; set; }
        [StringLength(10)]
        public string Prefix { get; set; }
        public long SequentialNumber { get; set; }
    }
}
