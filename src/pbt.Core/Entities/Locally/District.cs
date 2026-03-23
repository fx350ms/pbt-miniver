using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Entities.Locally
{
    public class District : Entity<int>
    {
        public int ProvinceId { get; set; }
        public string Name { get; set; }
    }
}
