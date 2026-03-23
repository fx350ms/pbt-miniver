using pbt.ApplicationUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.DeliveryNotes.Dto
{
    public class ScanCodeDto
    {
        public string Code { get; set; }
        public bool LockCustomer { get; set; } = false;
        public long? CustomerId { get; set; }
    }

    public class RemoveItemFromDeliveryNoteDto
    {
       
        public int ItemId { get; set; }
        public DeliveryNoteRemoveItemType ItemType { get; set; }
    }
}
