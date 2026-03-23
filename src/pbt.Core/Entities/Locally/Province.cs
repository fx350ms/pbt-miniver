using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Entities.Locally
{
    public class Province : Entity<int>
    {
        public string Name { get; set; }
    }
}
