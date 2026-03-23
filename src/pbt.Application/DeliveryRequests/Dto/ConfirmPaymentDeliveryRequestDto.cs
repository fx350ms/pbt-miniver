using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.DeliveryRequests.Dto
{
    public class ConfirmPaymentDeliveryRequestDto
    {
        public int DeliveryRequestId { get; set; }
        public int PaymentMethod { get; set; }
    }

    public class SubmitDeliveryRequestDto
    {
        public int Id { get; set; }
        public string Note { get; set; }
        public int ShippingMethod { get; set; }
        public string Address { get; set; }
    }
}
