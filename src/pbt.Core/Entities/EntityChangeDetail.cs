using System;
using Abp.Domain.Entities;

namespace pbt.Entities;

public class EntityChangeDetail : Entity<Guid>
{
    public Guid EntityChangeLogId { get; set; }
    public string PropertyName { get; set; }
    public string OriginalValue { get; set; }
    public string NewValue { get; set; }
}