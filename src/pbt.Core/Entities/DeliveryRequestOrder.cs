using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace pbt.Entities;

public class DeliveryRequestOrder :  FullAuditedEntity<int>
{
    public int DeliveryRequestId { get; set; }
    public long OrderId { get; set; }
    //public long? BagId { get; set; }
    [ForeignKey("DeliveryRequestId")]
    public DeliveryRequest DeliveryRequest { get; set; }
    [ForeignKey("OrderId")]

    public Order Order { get; set; }
}