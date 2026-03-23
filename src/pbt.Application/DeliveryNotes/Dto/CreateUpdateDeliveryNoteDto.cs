using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using JetBrains.Annotations;

namespace pbt.DeliveryNotes.Dto
{
    public class CreateUpdateDeliveryNoteDto : EntityDto<int>
    {
        public string Receiver { get; set; }
        public int? Status { get; set; }
        public long CustomerId { get; set; }
        public decimal? ShippingFee { get; set; }
        public decimal? DeliveryFee { get; set; }
        public decimal? FinancialNegativePart { get; set; }
        public decimal? Cod { get; set; }
        public int? length { get; set; }
        public int? width { get; set; }
        public int? height { get; set; }
        public string? Note { get; set; }
        public string RecipientPhoneNumber { get; set; }
        public string RecipientAddress { get; set; }
        [CanBeNull] public List<int> ItemBags { get; set; } = new List<int>();
        [CanBeNull] public List<int> ItemPackages { get; set; } = new List<int>();
        public int? DeliveryFeeReason { get; set; } 
        public int? ShippingPartnerId { get; set; } 
        public decimal? TotalWeight { get; set; } 
    }



}
