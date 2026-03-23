using System.Collections.Generic;
using System.Threading.Tasks;
using pbt.ChangeLogger.Dto;

namespace pbt.ChangeLogger;

public interface IEntityChangeLoggerAppService
{
    Task LogChangeAsync<TEntity>(
        TEntity oldEntity,
        TEntity newEntity,
        string action,
        string description = null,
        bool forceLogEvenNoChange = false
    ) where TEntity : class;

    public Task<List<LogEntryDto>> GetLogsAsync(string entityType, string entityId);
}