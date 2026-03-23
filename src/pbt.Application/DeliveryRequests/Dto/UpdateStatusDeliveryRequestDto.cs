using Abp.Application.Services.Dto;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace pbt.DeliveryRequests.Dto
{
	public class UpdateStatusDeliveryRequestDto
    {
		public int Id { get; set; }
		public int Status { get; set; }
	}
}
