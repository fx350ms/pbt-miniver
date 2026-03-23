using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Orders.Dto
{
    public class OrderSummaryDto
    {
        public int Total { get; set; }
        public int MyOrderTotal { get; set; }
        public int CustomerOrderTotal { get; set; }
        public int TotalPending { get; set; }
        public int TotalProcessing { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalCancel { get; set; }
    }
}
