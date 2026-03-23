using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Entities.Locally
{
    public class Ward : Entity<int>
    {
        public int DistrictId { get; set; }
        public string Name { get; set; }
    }
}
