using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Users.Dto
{
    public class UserWithHierarchyLevelDto
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public int HierarchyLevel { get; set; }
    }
}
