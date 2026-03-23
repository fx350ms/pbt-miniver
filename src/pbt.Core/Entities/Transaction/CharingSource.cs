using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Entities
{
    public class CharingSource : AuditedEntity<int>
    {
        public string Name { get; set; }
        public string Data { get; set; }
        public string Description { get; set; }
        public int SourceType { get; set; }
    }
}
