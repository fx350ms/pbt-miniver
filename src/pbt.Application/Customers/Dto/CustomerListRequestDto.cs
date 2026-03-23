using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Customers.Dto
{
    public class CustomerListRequestDto : PagedAndSortedResultRequestDto
    {
        public bool FilterCustomerBySale { get; set; } = false;
        public string Keyword { get; set; }
        public long? ParentId  { get; set; }
        public bool parentCustomer  { get; set; }
        public int? WarehouseId { get; set; }
        public int? SaleId { get; set; }
        public int CustomerType { get; set; } = -1;
        public int SaleType { get; set; } = -1;

    }
}
