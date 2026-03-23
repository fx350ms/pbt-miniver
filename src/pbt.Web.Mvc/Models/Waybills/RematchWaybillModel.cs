using pbt.Customers.Dto;
using pbt.Orders.Dto;
using System.Collections.Generic;
using System.Linq;

namespace pbt.Web.Models.Waybills
{
    public class RematchWaybillModel
    {
        public List<CustomerDto> Customers { get; set; }
        public List<WaybillForRematchDto> Waybills { get; set; }
        public long CurrentCustomerId { get; set; }
        public string OrderIds
        {
            get
            {
                if (Waybills == null || Waybills.Count == 0)
                {
                    return string.Empty;
                }
                else
                {
                    return string.Join(", ", Waybills.Select(w => w.Id));
                }
            }
        }
        public string WaybillNumberStr
        {
            get
            {
                if (Waybills == null || Waybills.Count == 0)
                {
                    return string.Empty;
                }
                else
                {
                    return string.Join(", ", Waybills.ConvertAll(w => w.WaybillNumber));
                }
            }
        }

    }
}
