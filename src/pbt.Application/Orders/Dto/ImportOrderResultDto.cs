using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Orders.Dto
{
    public class ImportOrderResultDto
    {
        public int Total { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<string> Messages { get; set; } = new List<string>();

    }
}
