using pbt.EntityAuditLogs.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.EntityAuditLogs
{
    public interface IEntityAuditLogApiClient
    {
        Task SendAsync(EntityAuditLogDto dto);
    }
}
