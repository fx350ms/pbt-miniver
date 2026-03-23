using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Customers.Dto
{
    public class CustomerWithWarehouseDto : EntityDto<long>
    {
        public string Username { get; set; }
        public int WarehouseId { get; set; }
        public  string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
    }
}
