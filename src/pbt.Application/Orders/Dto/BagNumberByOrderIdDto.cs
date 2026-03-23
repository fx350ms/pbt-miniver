using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Orders.Dto
{
    public class BagNumberByOrderIdDto
    {
        public long OrderId { get; set; }
        public string BagNumbers { get; set; }
    }
}
