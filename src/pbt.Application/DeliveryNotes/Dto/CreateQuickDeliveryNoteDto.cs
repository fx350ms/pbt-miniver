using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.DeliveryNotes.Dto
{
    public class CreateQuickDeliveryNoteDto
    {
        public long CustomerId { get; set; }
        public string Receiver { get; set; }
        public string RecipientPhoneNumber { get; set; }
        public string RecipientAddress { get; set; }
        public string Note { get; set; }
        public decimal TotalWeight { get; set; }
        public decimal DeliveryFee { get; set; }
        public int DeliveryFeeReason { get; set; }
        public int? ShippingPartnerId { get; set; }
        public List<DeliveryNoteItemDto> Items { get; set; }
    }

    public class DeliveryNoteItemDto
    {
        public int Id { get; set; }
        public int Type { get; set; } // 1: Bao, 2: Kiện
    }
}
