using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;
using pbt;
using pbt.ApplicationUtils;
using pbt.Authorization;
using pbt.Authorization.Users;
using pbt.ChangeLogger;
using pbt.ChangeLogger.Dto;
using pbt.Entities;

public class EntityChangeLoggerAppService : pbtAppServiceBase, IEntityChangeLoggerAppService
{
    private readonly IRepository<EntityChangeLog, Guid> _logRepository;
    private readonly IRepository<EntityChangeDetail, Guid> _detailRepository;
    private readonly pbtAppSession _pbtAppSession;
    private readonly UserManager _userManager;
    private readonly IRepository<ProductGroupType, int> _productGroupTypeRepository;

    public EntityChangeLoggerAppService(
        IRepository<EntityChangeLog, Guid> logRepository,
        IRepository<EntityChangeDetail, Guid> detailRepository,
        pbtAppSession pbtAppSession,
        UserManager userManager,
        IRepository<ProductGroupType, int> productGroupTypeRepository
        )
    {
        _logRepository = logRepository;
        _detailRepository = detailRepository;
        _pbtAppSession = pbtAppSession;
        _userManager = userManager;
        _productGroupTypeRepository = productGroupTypeRepository;
    }

    public async Task LogChangeAsync<TEntity>(
        TEntity oldEntity,
        TEntity newEntity,
        string action,
        string description = null,
        bool forceLogEvenNoChange = false)
        where TEntity : class
    {
        var entityType = typeof(TEntity).Name;
        var entityId = GetEntityId(newEntity ?? oldEntity);

        var log = new EntityChangeLog
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Description = description,
            CreatorId = _pbtAppSession.UserId,
            CreationTime = DateTime.Now,
            Details = new List<EntityChangeDetail>()
        };

        // ✅ Nếu oldEntity là null và forceLogEvenNoChange = true thì chỉ log header
        if (oldEntity == null && forceLogEvenNoChange)
        {
            await _logRepository.InsertAsync(log);
            return;
        }

        // Danh sách các thuộc tính không cần log
        var ignoreProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Id",
            "ConcurrencyStamp",
            "ExtraProperties",
            "CreationTime",
            "CreatorId",
            "LastModificationTime",
            "LastModifierId"
        };

        var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            if (!IsSimpleType(prop.PropertyType)) continue;
            if (ignoreProperties.Contains(prop.Name)) continue;

            var oldVal = oldEntity == null ? null : prop.GetValue(oldEntity)?.ToString();
            var newVal = newEntity == null ? null : prop.GetValue(newEntity)?.ToString();

            if (oldVal != newVal)
            {
                log.Details.Add(new EntityChangeDetail
                {
                    Id = Guid.NewGuid(),
                    EntityChangeLogId = log.Id,
                    PropertyName = prop.Name,
                    OriginalValue = oldVal,
                    NewValue = newVal
                });
            }
        }

        // Chỉ insert nếu có thay đổi hoặc được ép force log
        if (log.Details.Count > 0 || forceLogEvenNoChange)
        {
            await _logRepository.InsertAsync(log);

            if (log.Details.Any())
            {
                foreach (var detail in log.Details)
                {
                    await _detailRepository.InsertAsync(detail);
                }
            }
        }
    }

    private string GetEntityId(object entity)
    {
        var idProp = entity.GetType().GetProperty("Id");
        if (idProp == null) return null;

        var idVal = idProp.GetValue(entity);
        return idVal?.ToString();
    }

    private bool IsSimpleType(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return
            underlyingType.IsPrimitive ||
            new Type[]
            {
                typeof(string),
                typeof(decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(Guid)
            }.Contains(underlyingType) ||
            Convert.GetTypeCode(underlyingType) != TypeCode.Object;
    }

    public async Task<List<LogEntryDto>> GetLogsAsync(string entityType, string entityId)
    {
        var logs = (await _logRepository.GetAllAsync()).Include(x => x.Details);
        var filtered = logs
            .Where(x => x.EntityType == entityType && x.EntityId == entityId)
            .OrderByDescending(x => x.CreationTime)
            .ToList();

        var result = new List<LogEntryDto>();

        foreach (var log in filtered)
        {
            // get current user login
            var currentUser = await _userManager.GetUserByIdAsync(log.CreatorId ?? 0);

            var actor = currentUser.UserName ?? "Hệ thống";
            var description = BuildDescription(log);

            result.Add(new LogEntryDto
            {
                Actor = actor,
                Timestamp = log.CreationTime.ToString("dd/MM/yyyy HH:mm:ss"),
                Description = description
            });
        }

        return result;
    }



    public async Task<List<LogEntryDto>> GetLogsByMultiEntityTypeNameAsync(List<string> entityTypes, string entityId)
    {
        var logs = (await _logRepository.GetAllAsync()).Include(x => x.Details);
        var filtered = logs
            .Where(x => entityTypes.Contains(x.EntityType.ToUpper()) && x.EntityId == entityId)
            .OrderByDescending(x => x.CreationTime)
            .ToList();

        var result = new List<LogEntryDto>();

        foreach (var log in filtered)
        {
            // get current user login
            var currentUser = await _userManager.GetUserByIdAsync(log.CreatorId ?? 0);

            var actor = currentUser.UserName ?? "Hệ thống";
            var description = BuildDescription(log);

            result.Add(new LogEntryDto
            {
                Actor = actor,
                Timestamp = log.CreationTime.ToString("dd/MM/yyyy HH:mm:ss"),
                Description = description
            });
        }

        return result;
    }

    private string BuildDescription(EntityChangeLog log)
    {
        if (log.Details == null || log.Details.Count == 0)
            return log.Description ?? "[Không có chi tiết thay đổi]";

        var sb = new StringBuilder();

        if (!string.IsNullOrEmpty(log.Description))
        {
            sb.AppendLine($"{log.Description}:");
        }

        foreach (var detail in log.Details)
        {
            try
            {
                if (detail.PropertyName == "ProductGroupTypeId")
                {
                    // lấy tên từ enum thay vì id

                    var productGroupTypeOld = string.IsNullOrEmpty(detail.OriginalValue) ? null : _productGroupTypeRepository.GetAll().FirstOrDefault(x => x.Id == int.Parse(detail.OriginalValue));
                    var productGroupTypenew = string.IsNullOrEmpty(detail.NewValue) ? null : _productGroupTypeRepository.GetAll().FirstOrDefault(x => x.Id == int.Parse(detail.NewValue));

                    sb.AppendLine(
                        $"- {ToReadableName(detail.PropertyName)}: từ <b>{productGroupTypeOld?.Name}</b> thành <b>{productGroupTypenew?.Name}</b>");
                }
                else if (detail.PropertyName == "ShippingLineId")
                {
                    // lấy tên theo enum
                    var ShippingLineOld = ((LineShipping)int.Parse(detail.OriginalValue)).GetDescription();
                    var ShippingLineNew = ((LineShipping)int.Parse(detail.NewValue)).GetDescription();
                    sb.AppendLine(
                        $"- {ToReadableName(detail.PropertyName)}: từ <b>{ShippingLineOld}</b> thành <b>{ShippingLineNew}</b>");
                }
                else if (detail.PropertyName == "ShippingStatus")
                {
                    // lấy tên theo enum
                    var ShippingStatusOld = ((PackageDeliveryStatusEnum)int.Parse(detail.OriginalValue)).GetDescription();
                    var ShippingStatusNew = ((PackageDeliveryStatusEnum)int.Parse(detail.NewValue)).GetDescription();
                    sb.AppendLine(
                        $"- {ToReadableName(detail.PropertyName)}: từ <b>{ShippingStatusOld}</b> thành <b>{ShippingStatusNew}</b>");
                }
                else
                {
                    sb.AppendLine(
                        $"- {ToReadableName(detail.PropertyName)}: từ <b>{detail.OriginalValue}</b> thành <b>{detail.NewValue}</b>");
                }
            }
            catch (Exception ex)
            {
            }
        }
        return sb.ToString().Trim();
    }

    private string ToReadableName(string propName)
    {
        // Convert CamelCase to "Camel Case"
        return Regex.Replace(propName, "([a-z])([A-Z])", "$1 $2");
    }
}