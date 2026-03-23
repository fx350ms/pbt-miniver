using pbt.Customers.Dto;
using System.Collections.Generic;

namespace pbt.Web.Models.Waybills
{
    public class CreateWaybillListModel
    {
        public List<CustomerDto> Customers { get; set; } = new List<CustomerDto>();
    }
}
