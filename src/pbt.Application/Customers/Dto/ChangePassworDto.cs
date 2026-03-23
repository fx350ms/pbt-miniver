using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Customers.Dto
{
    public class ChangePasswordDto
    {
        public long CustomerId { get; set; }
        public string Password { get; set; }
    }
}
