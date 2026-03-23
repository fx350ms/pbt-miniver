using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Customers.Dto
{
    public class CustomerFinancialInfoDto : EntityDto<long>
    {
        public string UserName { get; set; }
        public long CustomerId { get; set; }
        public decimal CurrentDebt { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal MaxDebt { get; set; }
    }
}
