using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities.Auditing;

namespace pbt.Entities;

public class WarehouseDeliveryNote :  FullAuditedEntity<int>
{
    public string Receiver { get; set; }
}