using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using pbt.Customers.Dto;
using pbt.ShippingPartners.Dto;

namespace pbt.Web.Models.DeliveryNote
{
    public class CreateUpdateDeliveryNote
    {
        public CustomerDto CustomerDto { get; set; }
        public List<CustomerFinancialInfoDto> Customers { set; get; }
        public List<ShippingPartnerDto> ShippingPartners { set; get; }
        [CanBeNull] public string Address { set; get; }

    }

    public class CreateQuickDeliveryNote
    {
        public long CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerAddress { get; set; }

        public CustomerDto CustomerDto { get; set; }

        public int ShippingPartnerId { get; set; }
        public string ShippingPartnerName { get; set; }
        public string ShippingPartnerPhone { get; set; }
        public string ShippingPartnerEmail { get; set; }
        public string ShippingPartnerAddress { get; set; }
        public List<ShippingPartnerDto> ShippingPartners { set; get; }
        public List<CreateQuickDeliveryNoteItem> Items { get; set; } = new List<CreateQuickDeliveryNoteItem>();
        public decimal ShippingFee { get; set; } // Phí vận chuyển QT
        public decimal TotalWeight { get; set; } // Tổng trọng lượng của đơn hàng  
        public decimal DeliveryFee { get; set; } // Phí giao hàng (nếu có)
    }

    public class CreateQuickDeliveryNoteItem
    {
        public long Id { get; set; }
        public string Code { get; set; } // Code of the item (e.g., bag/package code)
        public int Type { get; set; } // Type of item (e.g., bag/package)   
        public decimal TotalWeight { get; set; } // Total weight of the item    
        public decimal TotalSize { get; set; } // Total size of the item
        //public decimal TotalAmount { get; set; } // Total amount of the item    
        public string Note { get; set; } // Note for the item
    }
}
