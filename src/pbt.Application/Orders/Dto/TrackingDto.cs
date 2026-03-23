using pbt.Packages.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Orders.Dto
{
    public class TrackingDto
    {

        public string TrackingNumber { get; set; }
        public int Status { get; set; }

        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }

        public OrderDto Order { get; set; }
        /// <summary>
        /// Tổng số kiện
        /// </summary>
        public int PackageCount { get; set; }

        public List<PackageDto> Packages { get; set; }
    }
}
