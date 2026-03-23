using System;
using System.Collections.Generic;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;

namespace pbt.Entities;

public class EntityChangeLog : Entity<Guid>
{
    public string EntityType { get; set; }
    public string EntityId { get; set; }
    public string Action { get; set; }
    public string Description { get; set; }
    public long? CreatorId { get; set; }
    public DateTime CreationTime { get; set; }

    public List<EntityChangeDetail> Details { get; set; } = new();
}