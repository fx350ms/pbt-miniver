using Abp.Application.Services.Dto;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace pbt.DeliveryRequests.Dto
{
	public class CreateUpdateDeliveryRequestDto
    {
		public List<long> Orders { get; set; }
		public List<int> Bags { get; set; }
		public int ShippingMethod { get; set; }
		public long CustomerId { get; set; }
		public long AddressId { get; set; }
		[CanBeNull] public string Note { get; set; }
	}
}
