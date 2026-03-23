using MassTransit;
using Microsoft.Extensions.Configuration;
using pbt.ApplicationUtils;
using pbt.EntityAuditLogs.Dto;
using System;
using System.Threading.Tasks;

namespace pbt.EntityAuditLogs
{
    //public class EntityAuditLogRabbitMqService : IEntityAuditLogService
    //{
    //   // private readonly IBus _bus;
    //    private readonly IConfiguration _config;

    //    public EntityAuditLogRabbitMqService(IBus bus, IConfiguration config)
    //    {
    //        _bus = bus;
    //        _config = config;
    //    }

    //    public async Task Publish(EntityAuditLogDto dto)
    //    {
    //        var queueName = _config["EntityAuditLog:QueueName"];

    //        var endpoint = await _bus.GetSendEndpoint(
    //            new Uri($"queue:{queueName}")
    //        );

    //        await endpoint.Send(dto);
    //        await _bus.Publish(dto);
    //    }
    //}
}
